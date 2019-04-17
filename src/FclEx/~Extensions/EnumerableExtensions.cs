using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FclEx.Utils;
using Microsoft.Collections.Extensions;
using MoreLinq;

namespace FclEx
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> NotNull<T>(this IEnumerable<T> col)
        {
            return col.Where(m => m != null);
        }

        public static IEnumerable<T> WhereIf<T>(
            this IEnumerable<T> source,
            Func<T, bool> predicate,
            bool condition)
        {
            return condition ? source.Where(predicate) : source;
        }

        public static IEnumerable<T> WhereIf<T>(
            this IEnumerable<T> source,
            Func<T, int, bool> predicate,
            bool condition)
        {
            return condition ? source.Where(predicate) : source;
        }

        public static Task<TResult[]> ForEachAsync<T, TResult>(
            this IEnumerable<T> sequence,
            Func<T, Task<TResult>> action)
        {
            return Task.WhenAll(sequence.Select(action).ToArray());
        }

        public static Task ForEachAsync<T>(
            this IEnumerable<T> sequence,
            Func<T, Task> action)
        {
            return Task.WhenAll(sequence.Select(action).ToArray());
        }

        public static (T[] True, T[] False) PartitionToArray<T>(
            this IEnumerable<T> source,
            Func<T, bool> predicate)
        {
            var pair = source.Partition(predicate);
            return (pair.True.ToArray(), pair.False.ToArray());
        }

        public static (List<T> True, List<T> False) PartitionToList<T>(
            this IEnumerable<T> source,
            Func<T, bool> predicate)
        {
            var pair = source.Partition(predicate);
            return (pair.True.ToList(), pair.False.ToList());
        }

        public static IEnumerable<TResult> SelectMany<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, TSource, TResult> resultSelector)
        {
            return source.SelectMany(m => source, resultSelector);
        }

        public static IEnumerable<T> Except<T>(
            this IEnumerable<T> source,
            T item,
            IEqualityComparer<T> comparer = null)
        {
            comparer = comparer ?? EqualityComparer<T>.Default;
            return source.Where(m => !comparer.Equals(m, item));
        }

        public static MultiValueDictionary<TKey, TValue> ToMultiValueDic<T, TKey, TValue>(
            this IEnumerable<T> enumerable,
            Func<T, TKey> keySelector,
            Func<T, IReadOnlyCollection<TValue>> valueSelector)
        {
            return new MultiValueDictionary<TKey, TValue>(enumerable.Select(m => KvPair.For(keySelector(m), valueSelector(m))));
        }

        public static MultiValueDictionary<TKey, TValue> ToMultiValueDic<T, TKey, TValue>(
            this IEnumerable<KeyValuePair<TKey, TValue>> enumerable)
        {
            var col = enumerable.GroupBy(m => m.Key)
                .Select(m => KvPair.For(m.Key, (IReadOnlyCollection<TValue>)m.Select(x => x.Value).ToArray()));
            return new MultiValueDictionary<TKey, TValue>(col);
        }

        public static OrderedDictionary<TKey, TValue> ToOrderedDic<T, TKey, TValue>(
            this IEnumerable<T> enumerable,
            Func<T, TKey> keySelector,
            Func<T, TValue> valueSelector)
        {
            return new OrderedDictionary<TKey, TValue>(enumerable.Select(m => KvPair.For(keySelector(m), valueSelector(m))));
        }

        public static OrderedDictionary<TKey, TValue> ToOrderedDic<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> enumerable)
        {
            return new OrderedDictionary<TKey, TValue>(enumerable);
        }
    }
}
