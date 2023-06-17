using Microsoft.Collections.Extensions;

namespace FclEx.Extensions;

partial class EnumerableExtensions
{
    public static MultiValueDictionary<TKey, TValue> ToMultiValueDic<T, TKey, TValue>(this IEnumerable<T> enumerable, Func<T, TKey> keySelector,
        Func<T, IEnumerable<TValue>> valueSelector, IEqualityComparer<TKey>? comparer = null)
    {
        var e = enumerable.Select(m => KeyValuePair.Create(keySelector(m), valueSelector(m)));
        return new MultiValueDictionary<TKey, TValue>(e, comparer);
    }

    public static MultiValueDictionary<TKey, TValue> ToMultiValueDic<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> enumerable, IEqualityComparer<TKey>? comparer = null)
    {
        var e = enumerable.GroupBy(m => m.Key)
            .Select(m => KvPair.Create(m.Key, m.Select(x => x.Value)));
        return new MultiValueDictionary<TKey, TValue>(e, comparer);
    }

    public static int Count<TKey, TValue>(this MultiValueDictionary<TKey, TValue> dic, TKey key)
    {
        return dic.GetOr(key)?.Count ?? 0;
    }

    public static bool IsEmpty<TKey, TValue>(this MultiValueDictionary<TKey, TValue> dic, TKey key)
    {
        return dic.Count(key) == 0;
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