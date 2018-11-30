using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using FclEx.TypeCasters;
using FclEx.Utils;

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
            return DynamicTypeCaster.Instance.CastTo<object, T>(obj);
        }

        public static TTarget CastTo<T, TTarget>(this T obj)
        {
            return ExpressionTypeCaster.Instance.CastTo<T, TTarget>(obj);
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

        public static int GetHashCodeSafely<T>(this T obj) where T : class
        {
            return obj == null ? 0 : obj.GetHashCode();
        }
    }
}
