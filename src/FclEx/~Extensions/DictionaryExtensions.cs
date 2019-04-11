using System;
using System.Collections.Generic;
using System.Linq;
using FclEx.Collections;

namespace FclEx
{
    public static class DictionaryExtensions
    {
        public static bool TryGetAndDo<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key,
            Action<TValue> action)
        {
            var result = dic.TryGetValue(key, out var value);
            if (result) action(value);
            return result;
        }

        public static TValue GetOrDefault<TKey, TValue>(
            this IDictionary<TKey, TValue> dic,
            TKey key,
            TValue defaultValue = default)
        {
            return dic.TryGetValue(key, out var value) ? value : defaultValue;
        }

        public static TProp GetOrDefault<TKey, TValue, TProp>(
            this IReadOnlyDictionary<TKey, TValue> dic,
            TKey key,
            Func<TValue, TProp> selector,
            TProp defaultValue = default)
        {
            return dic.TryGetValue(key, out var value) ? selector(value) : defaultValue;
        }

        public static TValue GetFirstOrDefault<TKey, TValue>(this MultiValueDictionary<TKey, TValue> dic, TKey key,
            TValue defaultValue = default)
        {
            return dic.TryGetValue(key, out var list) && list.Count > 0 ? list.First() : defaultValue;
        }

        public static void AddOrDo<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, TValue value, Action<TKey> action = null)
        {
            if (dic.ContainsKey(key)) action?.Invoke(key);
            else dic.Add(key, value);
        }

        public static void Add<TCol, TKey, TValue>(this IDictionary<TKey, TCol> dic, TKey key, TValue value) where TCol : ICollection<TValue>, new()
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

        public static void AddOrUpdateRange<TKey, TValue>(this IDictionary<TKey, TValue> dic, IEnumerable<KeyValuePair<TKey, TValue>> pairs)
        {
            foreach (var pair in pairs)
            {
                dic[pair.Key] = pair.Value;
            }
        }

        public static void AddOrUpdateRange<TKey, TValue>(this IDictionary<TKey, TValue> dic, IEnumerable<TValue> items, Func<TValue, TKey> func)
        {
            foreach (var item in items)
            {
                var key = func(item);
                dic[key] = item;
            }
        }

        public static void ReplaceBy<TKey, TValue>(this IDictionary<TKey, TValue> dic, IEnumerable<KeyValuePair<TKey, TValue>> pairs)
        {
            dic.Clear();
            AddOrUpdateRange(dic, pairs);
        }

        public static void ReplaceBy<TKey, TValue>(this IDictionary<TKey, TValue> dic, IEnumerable<TValue> items, Func<TValue, TKey> func)
        {
            dic.Clear();
            AddOrUpdateRange(dic, items, func);
        }

        public static bool GetAndDo<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, Action<TValue> action)
        {
            var item = dic.GetOrDefault(key);
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

        public static string ToUncodedQueryStr(this IEnumerable<KeyValuePair<string, string>> dic)
        {
            return dic.Select(m => $"{m.Key}={m.Value.GetOrEmpty()}").JoinWith("&");
        }

        public static string ToQueryStr(this IEnumerable<KeyValuePair<string, string>> dic)
        {
            return dic == null
                ? string.Empty
                : dic.Select(m => $"{m.Key.UrlEncode()}={m.Value.GetOrEmpty().UrlEncode()}").JoinWith("&");
        }
    }
}
