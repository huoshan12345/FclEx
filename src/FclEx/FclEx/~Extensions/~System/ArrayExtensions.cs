using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Dawn;

namespace FclEx
{
    public static class ArrayExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T>(this T[]? items, T item)
        {
            return items != null ? Array.IndexOf(items, item) : -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LastIndexOf<T>(this T[]? items, T item)
        {
            return items != null ? Array.LastIndexOf(items, item) : -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clear<T>(this T[] items)
        {
            Array.Clear(items, 0, items.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ArraySegment<T> ToSegmentOrEmpty<T>(this T[]? arr)
        {
            return new(arr ?? Array.Empty<T>());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ArraySegment<T> ToSegment<T>(this T[] arr)
        {
            return new(arr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ArraySegment<T> ToSegment<T>(this T[] arr, int offset, int count)
        {
            return new(arr, offset, count);
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

        public static IEnumerable<T> Concat<T>(this IEnumerable<IEnumerable<T>> arrays)
        {
            return arrays.SelectMany(m => m);
        }

        public static IEnumerable<T> Concat<T>(this T[] source, params IEnumerable<T>[] arrays)
        {
            return arrays.Prepend(source).Concat();
        }

        public static void ForEach<T>(this T[] array, Action<T> action)
        {
            Array.ForEach(array, action);
        }
    }
}
