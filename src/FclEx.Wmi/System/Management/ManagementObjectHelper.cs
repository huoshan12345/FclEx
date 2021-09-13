using System.Reflection;

namespace System.Management
{
    public static class ManagementObjectHelper
    {
        public static T ReadAs<T>(this ManagementObject obj) where T : new()
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
            public static readonly Func<ManagementObject, T> ReflectionConverter = BuildReflectionConverter();

            public static Func<ManagementObject, T> BuildReflectionConverter()
            {
                var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance);

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
}
