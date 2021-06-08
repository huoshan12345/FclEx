using System.Collections.Generic;

namespace FclEx
{
    partial class EnumerableExtensions
    {
        /// <summary>
        /// Wraps this object instance into an IEnumerable
        /// </summary>
        public static IEnumerable<T> Yield<T>(this T item)
        {
            yield return item;
        }

        public static IEnumerable<T> Yield<T>(this (T, T) items)
        {
            yield return items.Item1;
            yield return items.Item2;
        }

        public static IEnumerable<T> Yield<T>(this (T, T, T) items)
        {
            yield return items.Item1;
            yield return items.Item2;
            yield return items.Item3;
        }

        public static IEnumerable<T> Yield<T>(this (T, T, T, T) items)
        {
            yield return items.Item1;
            yield return items.Item2;
            yield return items.Item3;
            yield return items.Item4;
        }
    }
}
