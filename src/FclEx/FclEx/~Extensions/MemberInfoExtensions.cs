using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace FclEx
{
    public static class MemberInfoExtensions
    {
        public static bool IsDefined<T>(this MemberInfo memberInfo, bool inherit = true)
        {
            return Attribute.IsDefined(memberInfo, typeof(T), inherit);
        }

        public static DataMemberInfo ToDataMemberInfo(this MemberInfo memberInfo)
        {
            return memberInfo switch
            {
                PropertyInfo propInfo => new DataMemberInfo(propInfo),
                FieldInfo fieldInfo => new DataMemberInfo(fieldInfo),
                _ => throw new ArgumentException(
                    $"MemberInfo '{memberInfo.Name}' refers to neither a field nor a property.")
            };
        }
    }
}
