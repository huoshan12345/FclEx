using System.Collections.ObjectModel;

namespace FclEx.Extensions;

public static partial class DictionaryExtensions
{
    public static bool TryGetAndDo<TKey, TValue>([NotNullWhen(true)] this IDictionary<TKey, TValue>? dic, [NotNullWhen(true), MaybeNull] TKey key, Action<TValue> action)
    {
        if (key is null || dic is null)
            return false;

        var result = dic.TryGetValue(key, out var value);
        if (result)
        {
            action(value!);
        }
        return result;
    }

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static TValue? Get<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, TValue? defaultValue = default)
    {
        return dic.Get(key, k => defaultValue);
    }

    public static TValue? Get<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, Func<TKey, TValue?> fac)
    {
        return dic.TryGetValue(key, out var value) && value is not null ? value : fac(key);
    }

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static TProp? Get<TKey, TValue, TProp>(this IDictionary<TKey, TValue> dic, TKey key, Func<TValue, TProp> selector, TProp? defaultValue = default)
        where TProp : struct
    {
        return dic.TryGetValue(key, out var value) && value is not null ? selector(value) : defaultValue;
    }

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static TProp? Get<TKey, TValue, TProp>(this IDictionary<TKey, TValue> dic, TKey key, Func<TValue, TProp?> selector, TProp? defaultValue = default)
        where TProp : class
    {
        return dic.TryGetValue(key, out var value) && value is not null ? selector(value) : defaultValue;
    }

    public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey? key, TValue value) where TKey : notnull
    {
        return key is not null && dic.TryAdd(key, value);
    }

    public static void Add<TCol, TKey, TValue>(this IDictionary<TKey, TCol> dic, TKey key, TValue? value) where TCol : ICollection<TValue?>, new()
    {
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
            if (!dic.ContainsKey(key))
            {
                dic.Add(key, item);
            }
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
        if (!dic.TryGetValue(key, out var value))
        {
            value = valueFactory(key);
            dic[key] = value;
        }
        return value;
    }

    public static IReadOnlyDictionary<TKey, TValue> AsReadOnlyDictionary<TKey, TValue>(this IDictionary<TKey, TValue>? dic) where TKey : notnull
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
#endif
}