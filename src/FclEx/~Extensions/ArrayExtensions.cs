using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dawn;

namespace FclEx
{
    public static class ArrayExtensions
    {
        private static readonly Lazy<Random> _random = new Lazy<Random>(() => new Random());

        public static int IndexOf<T>([AllowNull]this T[] items, T item)
        {
            return items != null ? Array.IndexOf(items, item) : -1;
        }

        public static int LastIndexOf<T>([AllowNull]this T[] items, T item)
        {
            return items != null ? Array.LastIndexOf(items, item) : -1;
        }

        public static void Clear<T>(this T[] items)
        {
            Array.Clear(items, 0, items.Length);
        }

        public static void Shuffle<T>(this IList<T> list, Random? random = null)
        {
            Guard.Argument(list, nameof(list)).NotNull();
            var r = random ?? _random.Value;
            for (var i = list.Count - 1; i > 0; --i)
            {
                var randomIndex = r.Next(i + 1);
                T temp = list[i];
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }

        public static T GetAtOrDefault<T>([AllowNull]this IList<T> list, int index, T defaultValue = default(T))
        {
            return list != null && list.Count > index ? list[index] : defaultValue;
        }

        public static T GetRandomly<T>(this IList<T> list, Random? random = null)
        {
            Guard.Argument(list, nameof(list)).NotNull();
            var r = random ?? _random.Value;
            var i = r.Next(0, list.Count - 1);
            return list[i];
        }

        [return: MaybeNull]
        public static IList<T> TrySet<T>([AllowNull]this IList<T> list, int index, T value)
        {
            if (list != null && index >= 0 && index < list.Count)
                list[index] = value;
            return list;
        }

        public static ArraySegment<T> ToSegment<T>(this T[] arr)
        {
            return new ArraySegment<T>(arr);
        }

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

        public static bool IsNullOrEmpty<T>(this ArraySegment<T> source)
        {
            return source.Array.IsNullOrEmpty();
        }
    }
}
