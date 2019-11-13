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

        public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> source,
            Func<T, bool> predicate, bool condition)
        {
            return condition ? source.Where(predicate) : source;
        }

        public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> source,
            Func<T, int, bool> predicate, bool condition)
        {
            return condition ? source.Where(predicate) : source;
        }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public static IEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> source,
            Func<TSource, TSource, TResult> resultSelector)
        {
            return source.SelectMany(m => source, resultSelector);
        }

        public static IEnumerable<T> Except<T>(this IEnumerable<T> source,T item,IEqualityComparer<T> comparer = null)
        {
            comparer ??= EqualityComparer<T>.Default;
            return source.Where(m => !comparer.Equals(m, item));
        }
        
        /// <summary>
        /// 获取left和right的差集的第一个元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool TryGetFirstOfDiffSet<T>(this ICollection<T> left, IEnumerable<T> right, out T item)
        {
            item = default;
            foreach (var check in right)
            {
                if (!left.Contains(check))
                {
                    item = check;
                    return true;
                }
            }
            return false;
        }

        public static bool TryGetFirst<T>(this IEnumerable<T> source, out T value)
        {
            value = default;
            var items = source.Take(1).ToArray();
            if (items.Any())
            {
                value = items.First();
                return true;
            }
            return false;
        }

        public static bool TryGetFirst<T>(this IEnumerable<T> source, Func<T, bool> filter, out T value)
        {
            value = default;
            var items = source.Where(filter).Take(1).ToArray();
            if (items.Any())
            {
                value = items.First();
                return true;
            }
            return false;
        }
    }
}
