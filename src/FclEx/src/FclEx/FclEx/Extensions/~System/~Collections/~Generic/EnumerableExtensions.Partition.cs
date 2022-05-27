using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;

namespace FclEx.Extensions
{
    partial class EnumerableExtensions
    {
        public static (IEnumerable<TProp> True, IEnumerable<TProp> False) Partition<T, TProp>(this IEnumerable<T> enumerable,
               Func<T, bool> predicate, Func<T, TProp> selector)
        {
            var (@true, @false) = enumerable.Partition(predicate);
            return (@true.Select(selector), @false.Select(selector));
        }

        public static (TResult True, TResult False) Partition<T, TResult>(this IEnumerable<T> enumerable,
            Func<T, bool> predicate, Func<IEnumerable<T>, TResult> selector)
        {
            var (@true, @false) = enumerable.Partition(predicate);
            return (selector(@true), selector(@false));
        }

        public static (T[] True, T[] False) PartitionToArray<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            var (@true, @false) = source.Partition(predicate);
            return (@true.ToArray(), @false.ToArray());
        }

        public static (List<T> True, List<T> False) PartitionToList<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            var (@true, @false) = source.Partition(predicate);
            return (@true.ToList(), @false.ToList());
        }
    }
}
