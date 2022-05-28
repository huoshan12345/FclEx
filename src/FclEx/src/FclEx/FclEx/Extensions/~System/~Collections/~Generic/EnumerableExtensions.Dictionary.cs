using System;
using System.Collections.Generic;
using System.Linq;
using FclEx.Utils;
using Microsoft.Collections.Extensions;

namespace FclEx.Extensions;

partial class EnumerableExtensions
{
    public static MultiValueDictionary<TKey, TValue> ToMultiValueDic<T, TKey, TValue>(this IEnumerable<T> enumerable,
        Func<T, TKey> keySelector, Func<T, IEnumerable<TValue>> valueSelector)
    {
        return new MultiValueDictionary<TKey, TValue>(enumerable.Select(m => KvPair.Create(keySelector(m), valueSelector(m))));
    }

    public static MultiValueDictionary<TKey, TValue> ToMultiValueDic<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> enumerable)
    {
        var col = enumerable.GroupBy(m => m.Key)
            .Select(m => KvPair.Create(m.Key, m.Select(x => x.Value)));
        return new MultiValueDictionary<TKey, TValue>(col);
    }

    public static OrderedDictionary<TKey, TValue> ToOrderedDic<T, TKey, TValue>(this IEnumerable<T> enumerable,
        Func<T, TKey> keySelector, Func<T, TValue> valueSelector)
    {
        return new OrderedDictionary<TKey, TValue>(enumerable.Select(m => KvPair.Create(keySelector(m), valueSelector(m))));
    }

    public static OrderedDictionary<TKey, TValue> ToOrderedDic<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> enumerable)
    {
        return new OrderedDictionary<TKey, TValue>(enumerable);
    }

}