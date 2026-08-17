using System.Reflection.Emit;

namespace FclEx.Helpers;

public static class ReflectionHelper
{
    private static readonly OpCode?[] _oneByteOpCodes = new OpCode?[byte.MaxValue + 1];
    private static readonly OpCode?[] _twoByteOpCodes = new OpCode?[byte.MaxValue + 1];

    private const BindingFlags VisibleToDerived = BindingFlags.Public
                                            | BindingFlags.NonPublic
                                            | BindingFlags.Instance
                                            | BindingFlags.Static
                                            | BindingFlags.FlattenHierarchy;

    private const BindingFlags DeclaredNonPublic = BindingFlags.NonPublic
                                            | BindingFlags.Instance
                                            | BindingFlags.Static
                                            | BindingFlags.DeclaredOnly;

    static ReflectionHelper()
    {
        foreach (var field in typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (field.GetValue(null) is not OpCode opcode)
                continue;

            var value = unchecked((ushort)opcode.Value);
            if (opcode.Size == 1)
                _oneByteOpCodes[value] = opcode;
            else if (opcode.Size == 2 && value >> 8 == 0xFE)
                _twoByteOpCodes[(byte)value] = opcode;
        }
    }

    private static readonly ConditionalWeakTable<Type, IReadOnlyList<DataMemberInfo>> TypeDataMemberDic = new();

    private static readonly
#if NET9_0_OR_GREATER
            Lock
#else
            object
#endif
        _lock = new();

    public static IReadOnlyList<DataMemberInfo> GetDataMembers(Type type)
    {
        return TypeDataMemberDic.GetValue(type, GetDataMembersCore);

        static IReadOnlyList<DataMemberInfo> GetDataMembersCore(Type type)
        {
            if (type.IsInterface)
            {
                var members = type.GetInterfaces()
                    .Prepend(type)
                    .Select(GetDeclaredDataMembers)
                    .SelectMany(m => m);
                return members.ToReadOnlyList();
            }

            var list = new List<DataMemberInfo>(GetVisibleDataMembers(type));

            var baseType = type.BaseType;
            while (baseType is not null)
            {
                var members = GetNotVisibleToDerivedDataMembers(baseType);
                list.AddRange(members);
                baseType = baseType.BaseType;
            }

            return list.ToReadOnlyList();
        }

        static IEnumerable<DataMemberInfo> GetDeclaredDataMembers(Type type)
        {
            return type.GetMembers(BindingAttributes.Declared)
                .Where(m => m is PropertyInfo or FieldInfo)
                .Select(m => m.ToDataMemberInfo());
        }

        static IEnumerable<DataMemberInfo> GetVisibleDataMembers(Type type)
        {
            return type.GetMembers(VisibleToDerived)
                .Where(m => m is PropertyInfo or FieldInfo)
                .Select(m => m.ToDataMemberInfo());
        }

        static IEnumerable<DataMemberInfo> GetNotVisibleToDerivedDataMembers(Type type)
        {
            return type.GetMembers(DeclaredNonPublic)
                .Where(m => m is PropertyInfo property && property.IsNotVisibleToDerived()
                            || m is FieldInfo field && field.IsNotVisibleToDerived())
                .Select(m => m.ToDataMemberInfo());
        }
    }

    public static string GetAutoBackingFieldName(string propertyName)
    {
        return $"<{propertyName}>k__BackingField";
    }

    /// <summary>
    /// Determines whether the specified accessor method reads from, writes to, or takes the address of the specified field.
    /// </summary>
    /// <param name="method">The accessor method to inspect.</param>
    /// <param name="field">The field to check for usage.</param>
    /// <returns>
    /// <see langword="true"/> if the accessor contains an IL instruction that accesses the field;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The method decodes complete IL instructions before inspecting the next opcode. It recognizes <c>ldfld</c>,
    /// <c>stfld</c>, <c>ldflda</c>, <c>ldsfld</c>, <c>stsfld</c>, and <c>ldsflda</c> instructions only.
    /// Malformed or unsupported IL is treated as not accessing the field.
    /// </remarks>
    public static bool AccessorAccessesField(MethodInfo? method, FieldInfo field)
    {
        if (method?.DeclaringType is not { } declaringType)
            return false;

        if (declaringType != field.DeclaringType)
            return false;

        var body = method.GetMethodBody();
        var il = body?.GetILAsByteArray();
        if (il == null)
            return false;

        var isStatic = field.IsStatic;

        var genericTypeArgs = declaringType.IsGenericType ? declaringType.GetGenericArguments() : null;
        var genericMethodArgs = method.IsGenericMethod ? method.GetGenericArguments() : null;

        for (var offset = 0; offset < il.Length;)
        {
            if (TryReadInstruction(il, ref offset, out var opcode, out var token) == false)
                return false;

            if (token is not { } fieldToken || IsFieldAccess(opcode, isStatic) == false)
                continue;

            if (fieldToken == field.MetadataToken)
                return true;

            // Generic method bodies can use MemberRef tokens rather than the FieldDef token of the supplied FieldInfo.
            try
            {
                var resolvedField = declaringType.Module.ResolveField(fieldToken, genericTypeArgs, genericMethodArgs);
                if (field == resolvedField)
                    return true;
            }
            catch (ArgumentException) { }
        }

        return false;
    }

    private static bool IsFieldAccess(OpCode opcode, bool isStatic)
    {
        return isStatic
            ? opcode.Value is 0x7E /* ldsfld */ or 0x80 /* stsfld */ or 0x7F /* ldsflda */
            : opcode.Value is 0x7B /* ldfld */ or 0x7D /* stfld */ or 0x7C /* ldflda */;
    }

    /// <summary>
    /// Reads one complete IL instruction. Every CLR operand kind is skipped before the next opcode is considered,
    /// preventing opcode-like bytes within operands from being treated as instructions.
    /// </summary>
    private static bool TryReadInstruction(byte[] il, ref int offset, out OpCode opcode, out int? inlineFieldToken)
    {
        opcode = default;
        inlineFieldToken = null;

        if (offset >= il.Length)
            return false;

        var firstByte = il[offset++];
        OpCode? candidate;
        if (firstByte == 0xFE)
        {
            if (offset >= il.Length)
                return false;

            candidate = _twoByteOpCodes[il[offset++]];
        }
        else
        {
            candidate = _oneByteOpCodes[firstByte];
        }

        if (candidate is not { } readOpcode || TryGetOperandSize(il, offset, readOpcode.OperandType, out var operandSize) == false)
            return false;

        if (operandSize > il.Length - offset)
            return false;

        opcode = readOpcode;
        if (readOpcode.OperandType == OperandType.InlineField)
            inlineFieldToken = BitConverter.ToInt32(il, offset);

        offset += operandSize;
        return true;
    }

    private static bool TryGetOperandSize(byte[] il, int offset, OperandType operandType, out int operandSize)
    {
        switch (operandType)
        {
            case OperandType.InlineNone:
                operandSize = 0;
                return true;
            case OperandType.ShortInlineBrTarget:
            case OperandType.ShortInlineI:
            case OperandType.ShortInlineVar:
                operandSize = 1;
                return true;
            case OperandType.InlineVar:
                operandSize = 2;
                return true;
            case OperandType.InlineBrTarget:
            case OperandType.InlineField:
            case OperandType.InlineI:
            case OperandType.InlineMethod:
            case OperandType.InlineSig:
            case OperandType.InlineString:
            case OperandType.InlineTok:
            case OperandType.InlineType:
            case OperandType.ShortInlineR:
                operandSize = 4;
                return true;
            case OperandType.InlineI8:
            case OperandType.InlineR:
                operandSize = 8;
                return true;
            case OperandType.InlineSwitch:
                if (offset > il.Length - sizeof(int))
                {
                    operandSize = 0;
                    return false;
                }

                var branchCount = BitConverter.ToInt32(il, offset);
                var remainingBytes = il.Length - offset - sizeof(int);
                if (branchCount < 0 || branchCount > remainingBytes / sizeof(int))
                {
                    operandSize = 0;
                    return false;
                }

                operandSize = sizeof(int) + branchCount * sizeof(int);
                return true;
            default:
                operandSize = 0;
                return false;
        }
    }
}
