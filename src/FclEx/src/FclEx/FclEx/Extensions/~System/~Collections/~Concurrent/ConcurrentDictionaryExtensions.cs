using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace FclEx.Extensions
{
    public static class ConcurrentDictionaryExtensions
    {
        public static void Remove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dic, TKey key) where TKey : notnull
        {
            dic.TryRemove(key, out _);
        }

        public static ConcurrentDictionary<TKey, TValue> ToConcurrentDictionary<TKey, TValue>(
            this IEnumerable<KeyValuePair<TKey, TValue>> source) where TKey : notnull
        {
            return new(source);
        }

        public static ConcurrentDictionary<TKey, TValue> ToConcurrentDictionary<TKey, TValue>(
            this IEnumerable<TValue> source,
            Func<TValue, TKey> keySelector) where TKey : notnull
        {
            return new(
                from v in source
                select new KeyValuePair<TKey, TValue>(keySelector(v), v));
        }

        public static ConcurrentDictionary<TKey, TElement> ToConcurrentDictionary<TKey, TValue, TElement>(
            this IEnumerable<TValue> source,
            Func<TValue, TKey> keySelector,
            Func<TValue, TElement> elementSelector) where TKey : notnull
        {
            return new(
                from v in source
                select new KeyValuePair<TKey, TElement>(keySelector(v), elementSelector(v)));
        }
    }
}
