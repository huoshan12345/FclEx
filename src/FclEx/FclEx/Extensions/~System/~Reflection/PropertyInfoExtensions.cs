using System;
using System.Reflection;

namespace FclEx.Extensions
{
    public static class PropertyInfoExtensions
    {
        public static T? GetValue<T>(this PropertyInfo info, object? obj)
        {
            var value = info.GetValue(obj);
            return value == null ? default : (T)value;
        }

        public static MethodInfo GetRequiredGetMethod(this PropertyInfo prop)
        {
            return prop.GetGetMethod(true) ?? throw new MissingMethodException($"No getter in propery {prop.Name}");
        }
    }
}
