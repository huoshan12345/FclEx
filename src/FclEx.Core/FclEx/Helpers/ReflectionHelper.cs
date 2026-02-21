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
}