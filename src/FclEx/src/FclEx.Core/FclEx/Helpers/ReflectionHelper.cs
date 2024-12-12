namespace FclEx.Helpers;

public static class ReflectionHelper
{
    private static readonly ConcurrentDictionary<Type, Dictionary<string, DataMemberInfo>> TypeDataMemberDic = new();

    internal static Dictionary<string, DataMemberInfo> GetDataMembers(Type type)
    {
        return TypeDataMemberDic.GetOrAdd(type, GetDataMembersInternal);

        static Dictionary<string, DataMemberInfo> GetDataMembersInternal(Type type)
        {
            var list = new List<(DataMemberInfo Info, int Order)>();
            var cur = type;
            for (var i = 0; cur != null; i++)
            {
                var members = GetDeclaredDataMembers(cur);
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var member in members)
                {
                    list.Add((member, i));
                }
                cur = cur.BaseType;
            }

            // ReSharper disable once InvertIf
            if (type.IsInterface)
            {
                var ms = type.GetInterfaces()
                    .Select(GetDeclaredDataMembers)
                    .SelectMany(m => m)
                    .Select(m => (m, 1));
                list.AddRange(ms);
            }

            // use the most concrete member for the same name.
            return list.GroupBy(m => m.Info.Name)
                .ToDictionary(m => m.Key, m => m.MinimaBy(x => x.Order).Items[0].Info);
        }

        static IEnumerable<DataMemberInfo> GetDeclaredDataMembers(Type type)
        {
            return type.GetMembers(BindingAttributes.AllDeclared)
                .Where(m => m is PropertyInfo or FieldInfo)
                .Select(m => m.ToDataMemberInfo())
                .Where(m => m.IsCompilerGenerated == false);
        }
    }
}