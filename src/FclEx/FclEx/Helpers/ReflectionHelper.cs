using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DataMemberInfo = System.Reflection.DataMemberInfo;

namespace FclEx.Helpers
{
    public static class ReflectionHelper
    {
        private static readonly ConcurrentDictionary<Type, Dictionary<string, DataMemberInfo>> TypeDataMemberDic = new();

        internal static Dictionary<string, DataMemberInfo> GetDataMembers(Type type)
        {
            return TypeDataMemberDic.GetOrAdd(type, GetDataMembersInternal);

            static Dictionary<string, DataMemberInfo> GetDataMembersInternal(Type type)
            {
                var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                var members = props
                    .Select(m => new DataMemberInfo(m))
                    .Concat(fields.Select(m => new DataMemberInfo(m)))
                    .OrderBy(m => m.Name)
                    .ToList();
                return members.ToDictionary(m => m.Name);
            }
        }
    }
}
