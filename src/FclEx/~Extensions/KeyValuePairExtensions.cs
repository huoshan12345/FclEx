using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Dawn;
using FclEx.Extensions;
using FclEx.Utils;
using MoreLinq.Extensions;
using Newtonsoft.Json.Linq;

namespace FclEx
{
    public enum DupPolicy
    {
        OnlyLast = 0,
        OnlyFirst = 1,
        Throw = 2,
        Array
    }

    public static class KeyValuePairExtensions
    {
        public static void Deconstruct<TKey, TValue>(
            this KeyValuePair<TKey, TValue> kvp,
            out TKey key,
            out TValue value)
        {
            key = kvp.Key;
            value = kvp.Value;
        }

        public static JObject ToJObject(this IEnumerable<KeyValuePair<string, string>> pairs,
            DupPolicy policy = DupPolicy.OnlyLast)
        {
            var obj = new JObject();
            foreach (var pair in pairs.Where(m => m.Key != null).GroupBy(m => m.Key))
            {
                var values = pair.Select(m => m.Value).ToHashSet();
                if (values.Count > 0)
                    obj.Add(pair.Key, values.ToJToken(policy));
            }
            return obj;
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

        public static KeyValuePair<TKey, TValue> ValueOf<TKey, TValue>(this KeyValuePair<TKey, TValue> kv, TValue value)
        {
            return KvPair.Create(kv.Key, value);
        }
    }
}
