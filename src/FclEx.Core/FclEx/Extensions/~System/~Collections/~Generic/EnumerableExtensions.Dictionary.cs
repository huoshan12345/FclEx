namespace FclEx.Extensions;

partial class EnumerableExtensions
{
    public static MultiValueDictionary<TKey, TValue> ToMultiValueDictionary<T, TKey, TValue>(
        this IEnumerable<T> enumerable,
        Func<T, TKey> keySelector,
        Func<T, IEnumerable<TValue>> valueSelector,
        IEqualityComparer<TKey>? comparer = null)
        where TKey : notnull
    {
        var e = enumerable.Select(m => KeyValuePair.Create(keySelector(m), valueSelector(m)));
        return new MultiValueDictionary<TKey, TValue>(e, comparer);
    }

    public static MultiValueDictionary<TKey, TValue> ToMultiValueDictionary<TKey, TValue>(
        this IEnumerable<KeyValuePair<TKey, TValue>> enumerable,
        IEqualityComparer<TKey>? comparer = null)
            where TKey : notnull
    {
        var e = enumerable.GroupBy(m => m.Key)
            .Select(m => KeyValuePair.Create(m.Key, m.Select(x => x.Value)));
        return new MultiValueDictionary<TKey, TValue>(e, comparer);
    }

    public static MultiValueDictionary<TKey, TValue> Merge<TKey, TValue>(
        this IEnumerable<MultiValueDictionary<TKey, TValue>> enumerable,
        IEqualityComparer<TKey>? comparer = null)
        where TKey : notnull
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

    public static MultiValueDictionary<TKey, TValue> ToMultiValueDictionary<T, TKey, TValue, TValueCollection>(
        this IEnumerable<T> enumerable,
        Func<T, TKey> keySelector,
        Func<T, IEnumerable<TValue>> valueSelector,
        Func<TValueCollection> factory,
        IEqualityComparer<TKey>? comparer = null)
        where TKey : notnull
        where TValueCollection : ICollection<TValue>
    {
        var dic = MultiValueDictionary<TKey, TValue>.Create(comparer, factory);
        foreach (var item in enumerable)
        {
            dic.AddRange(keySelector(item), valueSelector(item));
        }
        return dic;
    }

    public static MultiValueDictionary<TKey, TValue> ToMultiValueDictionary<T, TKey, TValue>(
        this IEnumerable<T> enumerable,
        Func<T, TKey> keySelector,
        Func<T, TValue> valueSelector,
        IEqualityComparer<TKey>? comparer = null)
        where TKey : notnull
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

    public static MultiValueDictionary<TKey, T> ToMultiValueDictionary<T, TKey>(
        this IEnumerable<T> enumerable,
        Func<T, TKey> keySelector,
        IEqualityComparer<TKey>? comparer = null)
        where TKey : notnull
    {
        return enumerable.ToMultiValueDictionary(keySelector, m => m, comparer);
    }

    public static MultiValueDictionary<TKey, TValue> ToMultiValueDictionary<T, TKey, TValue, TValueCollection>(
        this IEnumerable<T> enumerable,
        Func<T, TKey> keySelector,
        Func<T, TValue> valueSelector,
        Func<TValueCollection> factory,
        IEqualityComparer<TKey>? comparer = null)
        where TKey : notnull
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

    public static MultiValueDictionary<TKey, T> ToMultiValueDictionary<T, TKey, TValueCollection>(
        this IEnumerable<T> enumerable,
        Func<T, TKey> keySelector,
        Func<TValueCollection> factory,
        IEqualityComparer<TKey>? comparer = null)
        where TKey : notnull
        where TValueCollection : ICollection<T>
    {
        return enumerable.ToMultiValueDictionary(keySelector, m => m, factory, comparer);
    }

    public static int Count<TKey, TValue>(this MultiValueDictionary<TKey, TValue> dic, TKey key) where TKey : notnull
    {
        return dic.Get(key)?.Count ?? 0;
    }

    public static bool IsEmpty<TKey, TValue>(this MultiValueDictionary<TKey, TValue> dic, TKey key) where TKey : notnull
    {
        return dic.Count(key) == 0;
    }

    public static OrderedDictionary<TKey, TValue> ToOrderedDictionary<T, TKey, TValue>(
        this IEnumerable<T> enumerable,
        Func<T, TKey> keySelector,
        Func<T, TValue> valueSelector)
        where TKey : notnull
    {
        return new OrderedDictionary<TKey, TValue>(enumerable.Select(m => KeyValuePair.Create(keySelector(m), valueSelector(m))));
    }

    public static OrderedDictionary<TKey, TValue> ToOrderedDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> enumerable)
        where TKey : notnull
    {
        return new OrderedDictionary<TKey, TValue>(enumerable);
    }

    public static BiDictionary<TKey, TValue> ToBiDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> enumerable,
        IEqualityComparer<TKey>? keyComparer = null, IEqualityComparer<TValue>? valueComparer = null)
        where TKey : notnull
        where TValue : notnull
    {
        var dic = new BiDictionary<TKey, TValue>(keyComparer, valueComparer);
        foreach (var (key, value) in enumerable)
        {
            dic[key] = value;
        }
        return dic;
    }

    public static BiDictionary<TKey, TValue> ToBiDictionary<T, TKey, TValue>(this IEnumerable<T> enumerable, Func<T, TKey> keySelector, Func<T, TValue> valueSelector,
        IEqualityComparer<TKey>? keyComparer = null, IEqualityComparer<TValue>? valueComparer = null)
        where TKey : notnull
        where TValue : notnull
    {
        var e = enumerable.Select(m => KeyValuePair.Create(keySelector(m), valueSelector(m)));
        return e.ToBiDictionary(keyComparer, valueComparer);
    }
}