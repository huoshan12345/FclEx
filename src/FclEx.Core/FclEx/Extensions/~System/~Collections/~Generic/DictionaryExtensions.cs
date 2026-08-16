namespace FclEx.Extensions;

public static partial class DictionaryExtensions
{
    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static TValue? Get<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, TValue? defaultValue = default)
    {
        return dic.TryGetValue(key, out var value) ? value : defaultValue;
    }

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static TMember? Get<TKey, TValue, TMember>(this IDictionary<TKey, TValue> dic, TKey key, Func<TValue, TMember> selector, TMember? defaultValue = default)
        where TMember : class
    {
        return dic.TryGetValue(key, out var value) ? selector(value) : defaultValue;
    }

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static TMember? Get<TKey, TValue, TMember>(this IDictionary<TKey, TValue> dic, TKey key, Func<TValue, TMember> selector, TMember? defaultValue = default)
        where TMember : struct
    {
        return dic.TryGetValue(key, out var value) ? selector(value) : defaultValue;
    }

    public static void Add<TCol, TKey, TValue>(this IDictionary<TKey, TCol> dic, TKey key, TValue? value) where TCol : ICollection<TValue?>, new()
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (dic.TryGetValue(key, out var col) && col is not null)
        {
            col.Add(value);
        }
        else
        {
            dic[key] = [value];
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
            dic.TryAdd(key, item);
        }
    }

    public static void Add<TKey, TValue, TCol>(this IDictionary<TKey, TCol> dic, TKey key, TValue value)
        where TCol : ICollection<TValue>, new()
    {
        if (!dic.TryGetValue(key, out var col))
        {
            col = [];
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
        if (dic.TryGetValue(key, out var value))
            return value;

        value = valueFactory(key);
        dic[key] = value;
        return value;
    }

    public static IReadOnlyDictionary<TKey, TValue> AsReadOnlyDictionary<TKey, TValue>(this IDictionary<TKey, TValue>? dic) where TKey : notnull
    {
        return dic switch
        {
            null => throw new ArgumentNullException(nameof(dic)),
            ReadOnlyDictionary<TKey, TValue> col => col,
            _ => new ReadOnlyDictionary<TKey, TValue>(dic)
        };
    }

    public static IReadOnlyDictionary<TKey, TValue> AsReadOnlyDictionaryView<TKey, TValue>(this IDictionary<TKey, TValue>? dic) where TKey : notnull
    {
        return dic switch
        {
            null => throw new ArgumentNullException(nameof(dic)),
            IReadOnlyDictionary<TKey, TValue> col => col,
            _ => new ReadOnlyDictionary<TKey, TValue>(dic)
        };
    }

#if !NET5_0_OR_GREATER
    public static bool Remove<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, [NotNullWhen(true)] out TValue value) where TKey : notnull
    {
        if (dic.TryGetValue(key, out value))
        {
            dic.Remove(key);
#pragma warning disable CS8762 // Parameter must have a non-null value when exiting in some condition.
            return true;
#pragma warning restore CS8762 // Parameter must have a non-null value when exiting in some condition.
        }
        return false;
    }

    public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
    {
        Check.NotNull(dictionary);

        if (dictionary.ContainsKey(key))
            return false;

        dictionary.Add(key, value);
        return true;
    }
#endif
}