using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using FclEx;

namespace FclEx
{
    public static class ObjectExtensions
    {
        public static bool IsDefault<T>(this T obj) => EqualityComparer<T>.Default.Equals(obj, default);

        public static bool IsNotDefault<T>(this T obj) => !IsDefault(obj);

        public static bool IsNull<T>(this T obj) => obj == null;

        public static bool IsNotNull<T>(this T obj) => !IsNull(obj);

        public static T CastTo<T>(this object obj)
        {
            var type = typeof(T);
            return type.IsValueType && !type.IsEnum
                ? (T)ChangeType(obj, type)
                : (T)obj;

            object ChangeType(object value, Type t)
            {
                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    if (value == null) return default;
                    t = Nullable.GetUnderlyingType(t);
                }
                return Convert.ChangeType(value, t);
            }
        }

        public static string ToTrimStringOrNull(this object obj)
        {
            return obj?.ToString().Trim();
        }

        public static string ToStringOrNull(this object obj)
        {
            return obj?.ToString();
        }

        public static string ToStringOrEmpty(this object obj)
        {
            return obj == null ? "" : obj.ToString();
        }

        public static int GetHashCodeSafely<T>(this T obj)
        {
            return obj == null ? 0 : obj.GetHashCode();
        }
    }
}
