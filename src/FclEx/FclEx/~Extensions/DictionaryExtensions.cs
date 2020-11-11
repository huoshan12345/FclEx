using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FclEx.Extensions;

namespace FclEx
{
    public static class DictionaryExtensions
    {
        public static bool TryGetAndDo<TKey, TValue>([NotNullWhen(true)] this IDictionary<TKey, TValue>? dic, [NotNullWhen(true), MaybeNull] TKey key, Action<TValue> action)
        {
            if (key == null || dic == null) return false;
            var result = dic.TryGetValue(key, out var value);
            if (result) action(value);
            return result;
        }

        [return: MaybeNull]
        public static TValue GetOr<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, TValue defaultValue = default)
        {
#pragma warning disable CS8620
            return dic.GetOr(key, k => defaultValue);
#pragma warning restore CS8620
        }

        [return: MaybeNull]
        public static TValue GetOr<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, Func<TKey, TValue> fac)
        {
            return dic.TryGetValue(key, out var value) && value != null ? value : fac(key);
        }

        [return: MaybeNull]
        public static TProp GetOr<TKey, TValue, TProp>(this IDictionary<TKey, TValue> dic, TKey key, Func<TValue, TProp> selector, TProp defaultValue = default)
        {
            return dic.TryGetValue(key, out var value) && value != null ? selector(value) : defaultValue;
        }

        [return: MaybeNull]
        public static TValue[] GetOrEmptyArr<TKey, TValue>(this IDictionary<TKey, TValue[]?> dic, TKey key)
        {
            return dic.GetOr(key, Array.Empty<TValue>());
        }

        public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, [AllowNull] TValue value)
        {
            if (key == null) return false;
            if (!dic.ContainsKey(key))
            {
#pragma warning disable 8604
                dic.Add(key, value);
#pragma warning restore 8604
                return true;
            }
            return false;
        }

        public static void Add<TCol, TKey, TValue>(this IDictionary<TKey, TCol> dic, TKey key, [AllowNull] TValue value) where TCol : ICollection<TValue>, new()
        {
            if (dic.ContainsKey(key) && dic[key] != null)
            {
#pragma warning disable 8604
                dic[key].Add(value);
#pragma warning restore 8604
            }
            else
            {
#pragma warning disable 8604
                dic[key] = new TCol { value };
#pragma warning restore 8604
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

        public static bool GetAndDo<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, Action<TValue> action)
        {
            if (key == null) return false;
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
            var item = dic.GetOr(key);
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
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

        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, Func<TKey, TValue> valueFactory)
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
