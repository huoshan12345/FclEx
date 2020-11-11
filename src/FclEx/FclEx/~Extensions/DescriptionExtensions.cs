using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;

namespace FclEx
{
    public static class DescriptionExtensions
    {
        public static string GetDescription(this Enum @enum)
        {
            var str = @enum.ToString();
            var field = @enum.GetType().GetField(str);
            return field == null ? str : GetDescription(field);
        }

        public static string GetDescription(this MemberInfo member)
        {
            var att = member.GetCustomAttribute<DescriptionAttribute>(false);
            return att == null ? member.Name : att.Description;
        }

        public static string GetDescription(this Type type)
        {
            var att = type.GetCustomAttribute<DescriptionAttribute>(false);
            return att == null ? type.Name : att.Description;
        }

        public static string GetFullDescription(this Enum en)
        {
            var type = en.GetType();
            var typeDesc = GetDescription(type);
            var enumDesc = en.GetDescription();
            return $"{typeDesc}-{enumDesc}";
        }
    }
}
