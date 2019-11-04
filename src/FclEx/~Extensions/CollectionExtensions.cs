using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dawn;
using FclEx.Utils;
using MoreLinq;

namespace FclEx
{
    public static class CollectionExtensions
    {
        public static int Remove<T>(this ICollection<T> col, Func<T, bool> filter)
        {
            return col.Count(item => filter(item) && col.Remove(item));
        }

        //public static int Remove<T>(this ICollection<T> col, Predicate<T> filter)
        //{
        //    return Remove(col, (Func<T, bool>)(m => filter(m)));
        //}

        public static bool IsEmpty<T>(this ICollection<T> col)
        {
            return col.Count == 0;
        }

        public static bool IsNullOrEmpty<T>(this ICollection<T> col)
        {
            return col == null || col.Count == 0;
        }

        public static void AddIfNotNull<T>(this ICollection<T> col, T item)
        {
            if (item != null) col.Add(item);
        }

        public static void AddRangeSafely<T>(this ICollection<T> col, IEnumerable<T> items)
        {
            if (items == null) return;
            if (col is List<T> list)
            {
                list.AddRange(items);
            }
            else
            {
                foreach (var item in items)
                {
                    col.Add(item);
                }
            }
        }

        public static IEnumerable<T> Touch<T>(this IEnumerable<T> col)
        {
            return col ?? Array.Empty<T>();
        }

        public static async Task Parallel<T>(this ICollection<T> superset, int pageSize,
            Action<T> action)
        {
            foreach (var items in superset.Batch(pageSize))
            {
                await items.Select(m => Task.Run(() => action(m))).WhenAll().DonotCapture();
            }
        }

        /// <summary>
        /// Adds an item to the collection if it's not already in the collection.
        /// </summary>
        /// <param name="source">Collection</param>
        /// <param name="item">Item to check and add</param>
        /// <typeparam name="T">Type of the items in the collection</typeparam>
        /// <returns>Returns True if added, returns False if not.</returns>
        public static bool AddIfNotContains<T>(this ICollection<T> source, T item)
        {
            Guard.Argument(source, nameof(source)).NotNull();

            if (source.Contains(item)) return false;

            source.Add(item);
            return true;
        }

        public static async Task<IReadOnlyList<TResult>> Parallel<T, TResult>(this ICollection<T> superset,
            int pageSize, Func<T, TResult> action)
        {
            var list = new List<TResult>(superset.Count);
            foreach (var items in superset.Batch(pageSize))
            {
                var r = await items
                    .Select(m => Task.Run(() => action(m)))
                    .WhenAll()
                    .DonotCapture();
                list.AddRange(r);
            }
            return list;
        }

        public static async Task Parallel<T>(this ICollection<T> superset,
            int pageSize, Func<T, Task> action)
        {
            foreach (var items in superset.Batch(pageSize))
            {
                await items.Select(action).WhenAll();
            }
        }

        public static async Task<IReadOnlyList<TResult>> Parallel<T, TResult>(this ICollection<T> superset,
            int pageSize, Func<T, Task<TResult>> action)
        {
            var list = new List<TResult>(superset.Count);
            foreach (var items in superset.Batch(pageSize))
            {
                var r = await items.Select(action).WhenAll();
                list.AddRange(r);
            }
            return list;
        }

        public static async ValueTask<IReadOnlyList<TResult>> Parallel<T, TResult>(this ICollection<T> superset,
            int pageSize, Func<T, ValueTask<TResult>> action)
        {
            var list = new List<TResult>(superset.Count);
            foreach (var items in superset.Batch(pageSize))
            {
                var r = await items.Select(action).WhenAll();
                list.AddRange(r);
            }
            return list;
        }
    }
}
