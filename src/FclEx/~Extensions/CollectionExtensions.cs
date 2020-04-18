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
        public static ICollection<T> AddIfNotNull<T>(this ICollection<T> source, T item)
        {
            Guard.Argument(source, nameof(source)).NotNull();
            if (!(item is null))
                source.Add(item);
            return source;
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
