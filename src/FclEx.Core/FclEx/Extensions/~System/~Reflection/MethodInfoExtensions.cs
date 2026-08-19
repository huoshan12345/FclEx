namespace FclEx.Extensions;

public static partial class MethodInfoExtensions
{
    private static readonly OpCode?[] _oneByteOpCodes = new OpCode?[byte.MaxValue + 1];
    private static readonly OpCode?[] _twoByteOpCodes = new OpCode?[byte.MaxValue + 1];

    static MethodInfoExtensions()
    {
        LoadOpCodes(_oneByteOpCodes, _twoByteOpCodes);
    }

    /// <summary>
    /// Determines whether the specified method reads from, writes to, or takes the address of the specified field.
    /// </summary>
    /// <param name="method">The method to inspect.</param>
    /// <param name="field">The field to check for usage.</param>
    /// <returns>
    /// <see langword="true"/> if the method contains an IL instruction that accesses the field;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The method decodes complete IL instructions before inspecting the next opcode. It recognizes <c>ldfld</c>,
    /// <c>stfld</c>, <c>ldflda</c>, <c>ldsfld</c>, <c>stsfld</c>, and <c>ldsflda</c> instructions only.
    /// Malformed or unsupported IL is treated as not accessing the field.
    /// </remarks>
    public static bool AccessesField(this MethodInfo method, FieldInfo field)
    {
        if (method.DeclaringType is not { } declaringType)
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

    private static void LoadOpCodes(OpCode?[] oneByteOpCodes, OpCode?[] twoByteOpCodes)
    {
        foreach (var field in typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (field.GetValue(null) is not OpCode opcode)
                continue;

            var value = (ushort)opcode.Value;
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (opcode.Size == 1)
                oneByteOpCodes[value] = opcode;
            else if (opcode.Size == 2 && value >> 8 == 0xFE)
                twoByteOpCodes[(byte)value] = opcode;
        }
    }

    [MethodImpl(AggressiveInlining)]
    public static bool IsAsync(this MethodInfo method)
    {
        // Obtain the custom attribute for the method.
        // The value returned contains the StateMachineType property.
        // Null is returned if the attribute isn't present for the method.
        return method.IsDefined<AsyncStateMachineAttribute>();
    }

    /// <summary>Gets a concise, human-readable representation of a method name and parameter types.</summary>
    public static string GetSignature(this MethodInfo method)
    {
        var paras = method.GetParameters();
        var name = method.GetFullName();
        var paraNames = paras.Select(m => m.ParameterType.LongName());

        return StringBuilderHelper.Build(m =>
        {
            m.Append(name);

            if (method.IsGenericMethod)
            {
                var genericArgs = method
                    .GetGenericArguments()
                    .Select(x => x.LongName());

                m.AppendAngleBracketed(x => x.AppendJoin(", ", genericArgs));
            }

            m.AppendCurlyBraced(x => x.AppendJoin(", ", paraNames));
        });
    }

    [MethodImpl(AggressiveInlining)]
    public static string GetFullName(this MethodInfo method)
    {
        return method.DeclaringType == null
            ? method.Name
            : $"{method.DeclaringType.LongName()}.{method.Name}";
    }

    [MethodImpl(AggressiveInlining)]
    public static T? Invoke<T>(this MethodInfo method, object? obj, object?[]? parameters)
    {
        return method.Invoke(obj, parameters).CastTo<T>();
    }

    [MethodImpl(AggressiveInlining)]
    public static T? InvokeInstance<T>(this MethodInfo method, object obj, params object?[]? parameters)
    {
        return method.Invoke<T>(obj, parameters);
    }

    [MethodImpl(AggressiveInlining)]
    public static T? InvokeStatic<T>(this MethodInfo method, params object?[]? parameters)
    {
        return method.Invoke<T>(null, parameters);
    }

#if !NET5_0_OR_GREATER
    [MethodImpl(AggressiveInlining)]
    public static T CreateDelegate<T>(this MethodInfo method) where T : Delegate
    {
        return (T)method.CreateDelegate(typeof(T));
    }

    [MethodImpl(AggressiveInlining)]
    public static T CreateDelegate<T>(this MethodInfo method, object? target) where T : Delegate
    {
        return (T)method.CreateDelegate(typeof(T), target);
    }
#endif

    extension(MethodInfo)
    {
        public static MethodInfo Of(Action action) => action.Method;
        public static MethodInfo Of<TResult>(Func<TResult> func) => func.Method;
    }
}
