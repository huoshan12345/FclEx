using System;
using System.Linq;
using System.Management;
using System.Reflection;

namespace FclEx.Wmi;

public static class ManagementObjectExtensions
{
    public static T ReadAs<T>(this ManagementBaseObject obj) where T : new()
    {
        return Cache<T>.ReflectionConverter.Invoke(obj);
    }

    public static T? Get<T>(this ManagementBaseObject obj, string key, T? defaultValue = default)
    {
        var value = obj.GetPropertyValue(key);
        return value == null
            ? defaultValue
            : (T)value;
    }

    internal static class Cache<T> where T : new()
    {
        public static readonly Func<ManagementBaseObject, T> ReflectionConverter = BuildReflectionConverter();

        public static Func<ManagementBaseObject, T> BuildReflectionConverter()
        {
            var fields = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(m => m is { CanRead: true, CanWrite: true });

            return m =>
            {
                var obj = new T();
                foreach (var field in fields)
                {
                    var v = m.GetPropertyValue(field.Name);
                    field.SetValue(obj, v);
                }
                return obj;
            };
        }
    }
}