using System;
using System.Collections.Generic;
using System.Linq;
using FclEx.Utils;
using Microsoft.Collections.Extensions;

namespace FclEx.Extensions
{
    public static class MultiValueDictionaryExtensions
    {
        public static TValue? GetFirstOr<TKey, TValue>(this MultiValueDictionary<TKey, TValue> dic, TKey key, TValue? defaultValue = default)
        {
            return dic.TryGetValue(key, out var list) && list.Count > 0 ? list.First() : defaultValue;
        }

        public static TValue? GetLastOr<TKey, TValue>(this MultiValueDictionary<TKey, TValue> dic, TKey key, TValue? defaultValue = default)
        {
            return dic.TryGetValue(key, out var list) && list.Count > 0 ? list.Last() : defaultValue;
        }

        public static IReadOnlyCollection<TValue>? GetOr<TKey, TValue>(this MultiValueDictionary<TKey, TValue> dic, TKey key, IReadOnlyCollection<TValue>? defaultValues = default)
        {
            return dic.TryGetValue(key, out var value) ? value : defaultValues;
        }

        public static IReadOnlyCollection<TValue> GetOrEmptyArr<TKey, TValue>(this MultiValueDictionary<TKey, TValue> dic, TKey key)
        {
            return dic.GetOr(key) ?? Array.Empty<TValue>();
        }

        public static TProp? GetOr<TKey, TValue, TProp>(this MultiValueDictionary<TKey, TValue> dic, TKey key, Func<IReadOnlyCollection<TValue>, TProp> selector, TProp? defaultValue = default)
        {
            return dic.TryGetValue(key, out var value) ? selector(value) : defaultValue;
        }

        public static KeyValuePair<string, string>[] ToPairs(this MultiValueDictionary<string, string> col)
        {
            return col.ToPair().ToArray();
        }

        public static IEnumerable<KeyValuePair<string, string>> ToPair(this MultiValueDictionary<string, string> col)
        {
            return col.SelectMany(m => m.Value, (k, v) => KvPair.Create(k.Key, v.ToStringOrEmpty()));
        }
    }
}
