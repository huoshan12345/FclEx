namespace FclEx.Helpers;

public static class ReflectionHelper
{
    private static readonly ConcurrentDictionary<Type, IReadOnlyList<DataMemberInfo>> TypeDataMemberDic = [];

    internal static IReadOnlyList<DataMemberInfo> GetDataMembers(Type type)
    {
        return TypeDataMemberDic.GetOrAdd(type, GetDataMembersInternal);

        static IReadOnlyList<DataMemberInfo> GetDataMembersInternal(Type type)
        {
            var list = new List<DataMemberInfo>();
            var cur = type;
            while (cur != null)
            {
                var members = GetDeclaredDataMembers(cur);
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var member in members)
                {
                    list.Add(member);
                }
                cur = cur.BaseType;
            }

            // ReSharper disable once InvertIf
            if (type.IsInterface)
            {
                var ms = type.GetInterfaces()
                    .Select(GetDeclaredDataMembers)
                    .SelectMany(m => m);
                list.AddRange(ms);
            }

            // use the most concrete member for the same name.
            return list.ToReadOnlyList();
        }

        static IEnumerable<DataMemberInfo> GetDeclaredDataMembers(Type type)
        {
            return type.GetMembers(BindingAttributes.AllDeclared)
                .Where(m => m is PropertyInfo or FieldInfo)
                .Select(m => m.ToDataMemberInfo());
        }
    }
}