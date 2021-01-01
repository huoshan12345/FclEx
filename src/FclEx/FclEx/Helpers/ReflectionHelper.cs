using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace FclEx.Helpers
{
    public static class ReflectionHelper
    {
        private static readonly ConcurrentDictionary<Type, Dictionary<string, DataMemberInfo>> TypeDataMemberDic = new();
        private const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        internal static Dictionary<string, DataMemberInfo> GetDataMembers(Type type)
        {
            return TypeDataMemberDic.GetOrAdd(type, GetDataMembersInternal);

            static Dictionary<string, DataMemberInfo> GetDataMembersInternal(Type type)
            {

                var members = new List<(DataMemberInfo Info, int Order)>();
                var t = type;
                for (var i = 0; t != null; i++)
                {
                    var ms = GetDeclaredDataMembers(t).Select(m => (m, i));
                    members.AddRange(ms);
                    t = t.BaseType;
                }

                if (type.IsInterface)
                {
                    var ms = type.GetInterfaces()
                        .Select(GetDeclaredDataMembers)
                        .SelectMany(m => m)
                        .Select(m => (m, 1));
                    members.AddRange(ms);
                }

                return members.GroupBy(m => m.Info.Name)
                    .ToDictionary(m => m.Key, m => m.OrderBy(x => x.Order).First().Info);
            }

            static IEnumerable<DataMemberInfo> GetDeclaredDataMembers(Type type)
            {
                return type.GetMembers(Flags)
                     .Where(m => m is PropertyInfo || m is FieldInfo)
                     .Select(m => m.ToDataMemberInfo())
                     .Where(m => !m.IsCompilerGenerated);
            }
        }
    }
}
