using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dawn;
using FclEx.Helpers;
using FclEx.Utils;
using MoreLinq;

namespace FclEx
{
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public static partial class EnumerableExtensions
    {
        public static bool IsValid<T>([AllowNull]this IEnumerable<T>? source)
        {
            return !source.IsNullOrEmpty();
        }

        public static bool IsEmpty<T>(this IEnumerable<T> source)
        {
            return !source.Any();
        }

        public static bool IsNullOrEmpty<T>([AllowNull]this IEnumerable<T>? source)
        {
            return source == null || !source.Any();
        }

        public static IEnumerable<T> Touch<T>([AllowNull]this IEnumerable<T> source)
        {
            return source ?? Enumerable.Empty<T>();
        }

        public static string JoinWith<T>(this IEnumerable<T> strs, string separator)
        {
            return string.Join(separator, strs.Select(m => m?.ToString()));
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

        public static IEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> source,
            Func<TSource, TSource, TResult> resultSelector)
        {
            return source.SelectMany(m => source, resultSelector);
        }

        public static IEnumerable<T> Except<T>(this IEnumerable<T> source, T item, IEqualityComparer<T>? comparer = null)
        {
            comparer ??= EqualityComparer<T>.Default;
            return source.Where(m => !comparer.Equals(m, item));
        }

        public static SortedSet<T> ToSortedSet<T>(this IEnumerable<T> enumerable, IComparer<T>? comparer = null)
        {
            return new SortedSet<T>(enumerable, comparer ?? Comparer<T>.Default);
        }

        public static ReadOnlyCollection<T> AsReadOnly<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable is ReadOnlyCollection<T> col) return col;
            var list = enumerable is IList<T> l ? l : enumerable.ToList();
            return new ReadOnlyCollection<T>(list);
        }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public static TProp[] ToArrayByIndex<T, TProp>(this IEnumerable<T> enumerable, Func<T, int> indexSelector, Func<T, TProp> valueSelector)
        {
            Guard.Argument(enumerable, nameof(enumerable)).NotNull();
            Guard.Argument(indexSelector, nameof(indexSelector)).NotNull();
            Guard.Argument(valueSelector, nameof(valueSelector)).NotNull();

            if (!enumerable.Any())
                return Array.Empty<TProp>();

            var max = enumerable.Max(indexSelector);
            var list = new TProp[max + 1];
            foreach (var item in enumerable)
            {
                var index = indexSelector(item);
                list[index] = valueSelector(item);
            }
            return list;
        }
    }
}
