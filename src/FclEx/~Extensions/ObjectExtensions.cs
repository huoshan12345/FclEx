using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using FclEx.TypeCasters;
using FclEx.Utils;

namespace FclEx
{
    public static class ObjectExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNull<T>(this T obj) => obj == null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotNull<T>(this T obj) => !obj.IsNull();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDefault<T>(this T obj) => EqualityComparer<T>.Default.Equals(obj, default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotDefault<T>(this T obj) => !IsDefault(obj);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T CastTo<T>(this object obj)
        {
            return DynamicTypeCaster.Instance.CastTo<object, T>(obj);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TTarget CastTo<T, TTarget>(this T obj)
        {
            return ExpressionTypeCaster.Instance.CastTo<T, TTarget>(obj);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToTrimStringOrNull(this object obj)
        {
            return obj?.ToString().Trim();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToStringOrNull<T>(this T obj)
        {
            return obj.IsNull() ? null : obj.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToStringOrEmpty<T>(this T obj)
        {
            return obj.IsNull() ? "" : obj.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetHashCodeSafely<T>(this T obj)
        {
            return obj.IsNull() ? 0 : obj.GetHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T DeepClone<T>(this T obj)
        {
            if (typeof(T).IsSerializable)
            {
                using (var ms = new MemoryStream())
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(ms, obj);
                    ms.Position = 0;
                    return (T)formatter.Deserialize(ms);
                }
            }
            else
            {
                return obj.ToJson().ToJToken().ToObject<T>();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T EnsureNotNull<T>(this T obj)
            where T : class, new()
        {
            return obj ?? new T();
        }
    }
}
