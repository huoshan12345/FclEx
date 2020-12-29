using System;
using System.Collections.Generic;
using System.Linq;
using FclEx.Comparers;
using FclEx.Helpers;

namespace FclEx
{
    public static class DistinctExtensions
    {
        public static IEnumerable<T> Distinct<T, V>(this IEnumerable<T> source, Func<T?, V> keySelector, IEqualityComparer<V>? comparer = null)
        {
            return source.Distinct(EqualityComparerHelper.Create(keySelector, comparer));
        }
    }
}
