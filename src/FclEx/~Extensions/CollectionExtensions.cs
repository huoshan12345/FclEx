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
        public static bool IsEmpty<T>(this ICollection<T> col)
        {
            return col.Count == 0;
        }

        public static bool IsNullOrEmpty<T>(this ICollection<T> col)
        {
            return col == null || col.Count == 0;
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
    }
}
