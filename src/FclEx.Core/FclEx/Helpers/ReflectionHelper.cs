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

    private static readonly ConcurrentDictionary<Type, IReadOnlyList<DataMemberInfo>> TypeDataMemberDic = [];

    public static IReadOnlyList<DataMemberInfo> GetDataMembers(Type type)
    {
        return TypeDataMemberDic.GetOrAdd(type, GetDataMembersInternal);

        static IReadOnlyList<DataMemberInfo> GetDataMembersInternal(Type type)
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

    public static bool AccessorUsesField(MethodInfo? method, FieldInfo field)
    {
        if (method is null)
            return false;

        if (method.IsCompilerGenerated() == false)
            return false;

        var body = method.GetMethodBody();
        var il = body?.GetILAsByteArray();
        if (il == null)
            return false;

        var fieldToken = field.MetadataToken;

        for (var i = 0; i < il.Length - 4; i++)
        {
            var op = il[i];

            if (op != 0x7B /* ldfld */ && op != 0x7D /* stfld */)
                continue;

            var token = BitConverter.ToInt32(il, i + 1);
            if (token == fieldToken)
                return true;
        }

        return false;
    }
}