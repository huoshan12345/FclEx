using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dawn;
using FclEx.Helpers;
using FclEx.Utils;
using Microsoft.Collections.Extensions;
using MoreLinq;

namespace FclEx
{
    public static partial class EnumerableExtensions
    {
        public static string JoinWith<T>(this IEnumerable<T> strs, string separator)
        {
            return string.Join(separator, strs.Select(m => m.ToString()));
        }

        public static IEnumerable<T> NotNull<T>(this IEnumerable<T> col)
        {
            return col.Where(m => m != null);
        }

        public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> source, Func<T, bool> predicate, bool condition)
        {
            return condition ? source.Where(predicate) : source;
        }

        public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> source, Func<T, int, bool> predicate, bool condition)
        {
            return condition ? source.Where(predicate) : source;
        }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public static IEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> source,
            Func<TSource, TSource, TResult> resultSelector)
        {
            return source.SelectMany(m => source, resultSelector);
        }

        public static IEnumerable<T> Except<T>(this IEnumerable<T> source, T item, IEqualityComparer<T> comparer = null)
        {
            comparer ??= EqualityComparer<T>.Default;
            return source.Where(m => !comparer.Equals(m, item));
        }
    }
}
