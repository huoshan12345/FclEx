using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FclEx
{
    public static class DictionaryExtensions
    {
        public static bool TryGetAndDo<TKey, TValue>([NotNullWhen(true)] this IDictionary<TKey, TValue>? dic, [NotNullWhen(true), MaybeNull] TKey key, Action<TValue> action)
        {
            if (key == null || dic == null) return false;
            var result = dic.TryGetValue(key, out var value);
            if (result) action(value!);
            return result;
        }

        public static TValue? Get<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, TValue? defaultValue = default)
        {
            return dic.Get(key, k => defaultValue);
        }

        public static TValue? Get<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, Func<TKey, TValue?> fac)
        {
            return dic.TryGetValue(key, out var value) && value != null ? value : fac(key);
        }

        public static TProp? Get<TKey, TValue, TProp>(this IDictionary<TKey, TValue> dic, TKey key, Func<TValue, TProp?> selector, TProp? defaultValue = default)
        {
            return dic.TryGetValue(key, out var value) && value != null ? selector(value) : defaultValue;
        }

        public static TValue[] GetOrEmptyArr<TKey, TValue>(this IDictionary<TKey, TValue[]> dic, TKey key)
        {
            return dic.Get(key) ?? Array.Empty<TValue>();
        }

        public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey? key, TValue value) where TKey : notnull
        {
            if (key == null) 
                return false;
            
            if (!dic.ContainsKey(key))
            {
                dic.Add(key, value);
                return true;
            }
            return false;
        }

        public static void Add<TCol, TKey, TValue>(this IDictionary<TKey, TCol> dic, TKey key, TValue? value) where TCol : ICollection<TValue?>, new()
        {
            if (dic.ContainsKey(key) && dic[key] != null)
            {
                dic[key].Add(value);
            }
            else
            {
                dic[key] = new TCol { value };
            }
        }

        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dic, IEnumerable<KeyValuePair<TKey, TValue>> pairs)
        {
            foreach (var pair in pairs)
            {
                if (!dic.ContainsKey(pair.Key))
                {
                    dic.Add(pair);
                }
            }
        }

        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dic, IEnumerable<TValue> items, Func<TValue, TKey> func)
        {
            foreach (var item in items)
            {
                var key = func(item);
                if (!dic.ContainsKey(key))
                {
                    dic.Add(key, item);
                }
            }
        }

        public static bool GetAndDo<TKey, TValue>(this IDictionary<TKey, TValue?> dic, TKey key, Action<TValue?> action)
        {
            if (key == null) return false;
            var item = dic.Get(key);
            if (item != null)
            {
                action(item);
                return true;
            }
            else return false;
        }

        public static void Add<TKey, TValue, TCol>(this IDictionary<TKey, TCol> dic, TKey key, TValue value)
            where TCol : ICollection<TValue>, new()
        {
            if (!dic.TryGetValue(key, out var col))
            {
                col = new TCol();
                dic[key] = col;
            }
            col.Add(value);
        }

        public static bool Remove<TKey, TValue, TCol>(this IDictionary<TKey, TCol> dic, TKey key, TValue value)
            where TCol : ICollection<TValue>, new()
        {
            return dic.TryGetValue(key, out var col) && (col?.Remove(value) ?? false);
        }

        public static bool Contains<TKey, TValue, TCol>(this IDictionary<TKey, TCol> dic, TKey key, TValue value)
            where TCol : ICollection<TValue>, new()
        {
            return dic.TryGetValue(key, out var col) && (col?.Contains(value) ?? false);
        }

        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, Func<TKey, TValue> valueFactory) where TKey : notnull
        {
            if (!dic.TryGetValue(key, out var value))
            {
                value = valueFactory(key);
                dic[key] = value;
            }
            return value;
        }
    }
}
