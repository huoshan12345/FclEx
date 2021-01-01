using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
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

        public static TTarget? Map<TSource, TTarget>(this TSource obj, Func<TSource, TTarget> func) => func(obj);

        public static object? GetMemberValue<T>(this T obj, string name)
        {
            return typeof(T).GetMemberValue<object>(name, obj);
        }

        public static TResult? GetMemberValue<T, TResult>(this T obj, string name)
        {
            return typeof(T).GetMemberValue<TResult>(name, obj);
        }

        public static T Between<T>(this T obj, T min, T max, IComparer<T>? comparer = null)
        {
            comparer ??= Comparer<T>.Default;
            if (comparer.Compare(min, max) > 0)
                throw new ArgumentOutOfRangeException(nameof(min), "The min value cannot be greater than the max value");

            if (comparer.Compare(obj, min) < 0)
                return min;
            if (comparer.Compare(obj, max) > 0)
                return max;
            return obj;
        }
    }
}
