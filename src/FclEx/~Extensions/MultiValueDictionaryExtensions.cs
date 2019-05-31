using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Collections.Extensions;

namespace FclEx
{
    public static class MultiValueDictionaryExtensions
    {
        public static TValue GetFirstOrDefault<TKey, TValue>(this MultiValueDictionary<TKey, TValue> dic,
            TKey key, TValue defaultValue = default)
        {
            return key != null && dic.TryGetValue(key, out var list) && list.Count > 0 ? list.First() : defaultValue;
        }

        public static IReadOnlyCollection<TValue> GetOrDefault<TKey, TValue>(this MultiValueDictionary<TKey, TValue> dic,
            TKey key, IReadOnlyCollection<TValue> defaultValues = default)
        {
            return key != null && dic.TryGetValue(key, out var value) ? value : defaultValues;
        }

        public static IReadOnlyCollection<TValue> GetOrEmptyArr<TKey, TValue>(this MultiValueDictionary<TKey, TValue> dic, TKey key)
        {
            return dic.GetOrDefault(key, Array.Empty<TValue>());
        }

        public static TProp GetOrDefault<TKey, TValue, TProp>(this MultiValueDictionary<TKey, TValue> dic,
            TKey key, Func<IReadOnlyCollection<TValue>, TProp> selector, TProp defaultValue = default)
        {
            return key != null && dic.TryGetValue(key, out var value) ? selector(value) : defaultValue;
        }
    }
}
