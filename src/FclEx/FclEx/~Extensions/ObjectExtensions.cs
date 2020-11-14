using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        [return: NotNullIfNotNull("obj"), MaybeNull]
        public static T CastTo<T>(this object? obj)
        {
            return DynamicTypeCaster.Instance.CastTo<object?, T>(obj);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNullIfNotNull("obj"), MaybeNull]
        public static TTarget CastTo<T, TTarget>([MaybeNull] this T obj)
        {
            return ExpressionTypeCaster.Instance.CastTo<T, TTarget>(obj);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToStringOrEmpty<T>([MaybeNull] this T obj)
        {
            return obj is null ? string.Empty : obj.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetHashCodeSafely<T>([MaybeNull] this T obj)
        {
            return obj is null ? 0 : obj.GetHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNullIfNotNull("obj"), MaybeNull]
        public static T DeepClone<T>([MaybeNull] this T obj)
        {
            if (obj is null)
                return obj;

            if (typeof(T).IsSerializable)
            {
                using var ms = new MemoryStream();
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;
                return (T)formatter.Deserialize(ms);
            }
            else
            {
                return obj.ToJson().ToJToken().ToObject<T>()!;
            }
        }
    }
}
