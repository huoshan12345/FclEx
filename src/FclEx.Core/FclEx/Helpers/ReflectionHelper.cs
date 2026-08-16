namespace FclEx.Helpers;

public static class ReflectionHelper
{
    private const BindingFlags VisibleToDerived = BindingFlags.Public
                                            | BindingFlags.NonPublic
                                            | BindingFlags.Instance
                                            | BindingFlags.Static
                                            | BindingFlags.FlattenHierarchy;

    private const BindingFlags DeclaredNonPublic = BindingFlags.NonPublic
                                            | BindingFlags.Instance
                                            | BindingFlags.Static
                                            | BindingFlags.DeclaredOnly;

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
    /// Determines whether the specified accessor method reads from or writes to the specified field.
    /// </summary>
    /// <param name="method">The accessor method to inspect.</param>
    /// <param name="field">The field to check for usage.</param>
    /// <returns>
    /// <see langword="true"/> if the accessor contains an IL instruction that accesses the field;
    /// otherwise, <see langword="false"/>.
    /// </returns>
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

        var fieldToken = field.MetadataToken;
        var isStatic = field.IsStatic;

        var genericTypeArgs = declaringType.IsGenericType ? declaringType.GetGenericArguments() : null;
        var genericMethodArgs = method.IsGenericMethod ? method.GetGenericArguments() : null;

        for (var i = 0; i < il.Length - 4; i++)
        {
            var op = il[i];

            if (isStatic)
            {
                if (op != 0x7E /* ldsfld */ && op != 0x80 /* stsfld */)
                    continue;
            }
            else
            {
                if (op != 0x7B /* ldfld */ && op != 0x7D /* stfld */)
                    continue;
            }

            var token = BitConverter.ToInt32(il, i + 1);

            if (token == fieldToken)
                return true;

            // Attempt to resolve the field token to a FieldInfo and compare it with the provided field.
            try
            {
                var resolveField = declaringType.Module.ResolveField(token, genericTypeArgs, genericMethodArgs);
                if (field == resolveField)
                    return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Failed to resolve field: " + ex);
                continue;
            }
        }

        return false;
    }
}