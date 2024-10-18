using Microsoft.Collections.Extensions;

namespace FclEx.Extensions;

partial class EnumerableExtensions
{
    public static MultiValueDictionary<TKey, TValue> ToMultiValueDictionary<T, TKey, TValue>(this IEnumerable<T> enumerable,
        Func<T, TKey> keySelector, Func<T, IEnumerable<TValue>> valueSelector, IEqualityComparer<TKey>? comparer = null)
    {
        var e = enumerable.Select(m => KeyValuePair.Create(keySelector(m), valueSelector(m)));
        return new MultiValueDictionary<TKey, TValue>(e, comparer);
    }

    public static MultiValueDictionary<TKey, TValue> ToMultiValueDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> enumerable, IEqualityComparer<TKey>? comparer = null)
    {
        var e = enumerable.GroupBy(m => m.Key)
            .Select(m => KeyValuePair.Create(m.Key, m.Select(x => x.Value)));
        return new MultiValueDictionary<TKey, TValue>(e, comparer);
    }

    public static MultiValueDictionary<TKey, TValue> Merge<TKey, TValue>(this IEnumerable<MultiValueDictionary<TKey, TValue>> enumerable, IEqualityComparer<TKey>? comparer = null)
    {
        var dic = new MultiValueDictionary<TKey, TValue>(comparer);
        foreach (var item in enumerable)
        {
            foreach (var (key, value) in item)
            {
                dic.AddRange(key, value);
            }
        }
        return dic;
    }

    public static MultiValueDictionary<TKey, TValue> ToMultiValueDictionary<T, TKey, TValue, TValueCollection>(this IEnumerable<T> enumerable,
        Func<T, TKey> keySelector, Func<T, IEnumerable<TValue>> valueSelector, Func<TValueCollection> factory, IEqualityComparer<TKey>? comparer = null)
        where TValueCollection : ICollection<TValue>
    {
        var dic = MultiValueDictionary<TKey, TValue>.Create(comparer, factory);
        foreach (var item in enumerable)
        {
            dic.AddRange(keySelector(item), valueSelector(item));
        }
        return dic;
    }

    public static MultiValueDictionary<TKey, TValue> ToMultiValueDictionary<T, TKey, TValue>(this IEnumerable<T> enumerable,
        Func<T, TKey> keySelector, Func<T, TValue> valueSelector, IEqualityComparer<TKey>? comparer = null)
    {
        var dic = new MultiValueDictionary<TKey, TValue>(comparer);
        foreach (var item in enumerable)
        {
            var key = keySelector(item);
            var value = valueSelector(item);
            dic.Add(key, value);
        }
        return dic;
    }

    public static MultiValueDictionary<TKey, TValue> ToMultiValueDictionary<T, TKey, TValue, TValueCollection>(this IEnumerable<T> enumerable,
        Func<T, TKey> keySelector, Func<T, TValue> valueSelector, Func<TValueCollection> factory, IEqualityComparer<TKey>? comparer = null)
        where TValueCollection : ICollection<TValue>
    {
        var dic = MultiValueDictionary<TKey, TValue>.Create(comparer, factory);
        foreach (var item in enumerable)
        {
            var key = keySelector(item);
            var value = valueSelector(item);
            dic.Add(key, value);
        }
        return dic;
    }

    public static int Count<TKey, TValue>(this MultiValueDictionary<TKey, TValue> dic, TKey key)
    {
        return dic.Get(key)?.Count ?? 0;
    }

    public static bool IsEmpty<TKey, TValue>(this MultiValueDictionary<TKey, TValue> dic, TKey key)
    {
        return dic.Count(key) == 0;
    }

    public static OrderedDictionary<TKey, TValue> ToOrderedDictionary<T, TKey, TValue>(this IEnumerable<T> enumerable,
        Func<T, TKey> keySelector, Func<T, TValue> valueSelector)
    {
        return new OrderedDictionary<TKey, TValue>(enumerable.Select(m => KvPair.Create(keySelector(m), valueSelector(m))));
    }

    public static OrderedDictionary<TKey, TValue> ToOrderedDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> enumerable)
    {
        return new OrderedDictionary<TKey, TValue>(enumerable);
    }
}