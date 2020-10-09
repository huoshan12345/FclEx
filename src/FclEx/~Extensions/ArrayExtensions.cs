using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Dawn;

namespace FclEx
{
    public static class ArrayExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T>([AllowNull] this T[] items, T item)
        {
            return items != null ? Array.IndexOf(items, item) : -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LastIndexOf<T>([AllowNull] this T[] items, T item)
        {
            return items != null ? Array.LastIndexOf(items, item) : -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clear<T>(this T[] items)
        {
            Array.Clear(items, 0, items.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ArraySegment<T> ToSegment<T>(this T[] arr)
        {
            return new ArraySegment<T>(arr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ArraySegment<T> ToSegment<T>(this T[] arr, int offset, int count)
        {
            return new ArraySegment<T>(arr, offset, count);
        }

        public static IList<T> ConvertOrToIList<T>(this IEnumerable<T> raw)
        {
            if (raw is List<T> list) return list;
            if (raw is T[] arr) return arr;
            return raw.ToList();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty<T>(this ArraySegment<T> source)
        {
            return source.Array.IsNullOrEmpty();
        }

        public static IEnumerable<ArraySegment<T>> Segments<T>(this T[] array, int maxSize)
        {
            Guard.Argument(array, nameof(array)).NotNull();
            Guard.Argument(maxSize, nameof(maxSize)).GreaterThan(0, (value, other) => "The size of segment cannot be less than " + other);

            var count = (array.Length - 1) / maxSize + 1;
            for (var i = 0; i < count; i++)
            {
                var length = i + 1 == count ? array.Length - i * maxSize : maxSize;
                yield return array.ToSegment(i * maxSize, length);
            }
        }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public static T[] Concat<T>(this IEnumerable<T[]> arrays)
        {
            Guard.Argument(arrays, nameof(arrays)).NotNull();

            var len = 0;
            foreach (var array in arrays)
            {
                Guard.Argument(array, nameof(array)).NotNull();
                len += array.Length;
            }

            var z = new T[len];
            var index = 0;
            foreach (var array in arrays)
            {
                array.CopyTo(z, index);
                index += array.Length;
            }
            return z;
        }

        public static T[] Concat<T>(this T[] source, params T[][] arrays)
        {
            return arrays.Prepend(source).Concat();
        }
    }
}
