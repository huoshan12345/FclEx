namespace FclEx.Extensions;

public static class MultiValueDictionaryExtensions
{
    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static TValue? GetFirst<TKey, TValue>(this MultiValueDictionary<TKey, TValue> dic, TKey key, TValue? defaultValue = default)
    {
        return dic.TryGetValue(key, out var list) && list.Count > 0 ? list.First() : defaultValue;
    }

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static TValue? GetLast<TKey, TValue>(this MultiValueDictionary<TKey, TValue> dic, TKey key, TValue? defaultValue = default)
    {
        return dic.TryGetValue(key, out var list) && list.Count > 0 ? list.Last() : defaultValue;
    }

    [return: NotNullIfNotNull(nameof(defaultValues))]
    public static IReadOnlyCollection<TValue>? Get<TKey, TValue>(this MultiValueDictionary<TKey, TValue> dic, TKey key, IReadOnlyCollection<TValue>? defaultValues)
    {
        // ReSharper disable once CanSimplifyDictionaryTryGetValueWithGetValueOrDefault
        return dic.TryGetValue(key, out var values) ? values : defaultValues;
    }

    public static IReadOnlyCollection<TValue> Get<TKey, TValue>(this MultiValueDictionary<TKey, TValue> dic, TKey key)
    {
        return dic.Get(key, []);
    }

    public static void Set<TKey, TValue>(this MultiValueDictionary<TKey, TValue> dic, TKey key, TValue value)
    {
        dic.Remove(key);
        dic.Add(key, value);
    }

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static TProp? Get<TKey, TValue, TProp>(this MultiValueDictionary<TKey, TValue> dic, TKey key, Func<IReadOnlyCollection<TValue>, TProp> selector, TProp? defaultValue = default)
    {
        return dic.TryGetValue(key, out var value) ? selector(value) : defaultValue;
    }

    public static IEnumerable<KeyValuePair<string, string>> ToPair(this MultiValueDictionary<string, string> col)
    {
        return col.SelectMany(m => m.Value, (k, v) => KeyValuePair.Create(k.Key, v));
    }

    public static void AddRange<TKey, TValue, TCol>(this MultiValueDictionary<TKey, TValue> dic, IEnumerable<KeyValuePair<TKey, TCol>> pairs) where TCol : IEnumerable<TValue>
    {
        foreach (var (key, value) in pairs)
        {
            dic.AddRange(key, value);
        }
    }
}