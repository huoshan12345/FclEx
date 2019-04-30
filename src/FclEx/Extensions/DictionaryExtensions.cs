using System;
using System.Collections.Generic;
using System.Text;

namespace FclEx.Extensions
{
    public static class DictionaryExtensions
    {
        public static void AddOrDo<TKey, TValue>(this IDictionary<TKey, TValue> dic,
            TKey key, TValue value, Action<TKey> action = null)
        {
            if (dic.ContainsKey(key)) action?.Invoke(key);
            else dic.Add(key, value);
        }

        public static void AddOrUpdateRange<TKey, TValue>(this IDictionary<TKey, TValue> dic,
            IEnumerable<KeyValuePair<TKey, TValue>> pairs)
        {
            foreach (var pair in pairs)
            {
                dic[pair.Key] = pair.Value;
            }
        }

        public static void AddOrUpdateRange<TKey, TValue>(this IDictionary<TKey, TValue> dic,
            IEnumerable<TValue> items, Func<TValue, TKey> func)
        {
            foreach (var item in items)
            {
                var key = func(item);
                dic[key] = item;
            }
        }

        public static void ReplaceBy<TKey, TValue>(this IDictionary<TKey, TValue> dic,
            IEnumerable<KeyValuePair<TKey, TValue>> pairs)
        {
            dic.Clear();
            AddOrUpdateRange(dic, pairs);
        }

        public static void ReplaceBy<TKey, TValue>(this IDictionary<TKey, TValue> dic,
            IEnumerable<TValue> items, Func<TValue, TKey> func)
        {
            dic.Clear();
            AddOrUpdateRange(dic, items, func);
        }
    }
}
