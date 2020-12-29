using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using FclEx.TypeCasters;

namespace FclEx
{
    public static class ObjectExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNullIfNotNull("obj"), MaybeNull]
        public static T CastTo<T>(this object? obj)
        {
            return DynamicTypeCaster.Instance.CastTo<object?, T>(obj);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNullIfNotNull("obj"), MaybeNull]
        public static TTarget CastTo<T, TTarget>(this T? obj)
        {
            return ExpressionTypeCaster.Instance.CastTo<T, TTarget>(obj);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToStringOrEmpty<T>(this T? obj)
        {
            return obj?.ToString() ?? string.Empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetHashCodeSafely<T>([AllowNull] this T obj)
        {
            return obj is null ? 0 : obj.GetHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNullIfNotNull("obj"), MaybeNull]
        public static T DeepClone<T>([AllowNull] this T obj)
        {
            return obj.ToJson().ToJToken().ToObject<T>()!;
        }

        [return: MaybeNull]
        public static TTarget Map<TSource, TTarget>(this TSource obj, Func<TSource, TTarget> func) => func(obj);
    }
}
