using System;
using System.Collections.Generic;

namespace FclEx
{
    public static class ArrayExtensions
    {
        private static readonly Lazy<Random> _random = new Lazy<Random>(() => new Random());

        public static int IndexOf<T>(this T[] items, T item)
        {
            return items != null ? Array.IndexOf(items, item) : -1;
        }

        public static int LastIndexOf<T>(this T[] items, T item)
        {
            return items != null ? Array.LastIndexOf(items, item) : -1;
        }

        public static void Clear<T>(this T[] items)
        {
            if (items != null)
            {
                Array.Clear(items, 0, items.Length);
            }
        }

        public static T GetAtOrDefault<T>(this IList<T> list, int index, T defaultValue = default(T))
        {
            return list != null && list.Count > index ? list[index] : defaultValue;
        }

        public static T Random<T>(this IList<T> col, Random random = null)
        {
            var r = random ?? _random.Value;
            var i = r.Next(0, col.Count - 1);
            return col[i];
        }

        public static IList<T> TrySet<T>(this IList<T> list, int index, T value)
        {
            if (list != null && index >= 0 && index < list.Count)
                list[index] = value;
            return list;
        }
    }
}
