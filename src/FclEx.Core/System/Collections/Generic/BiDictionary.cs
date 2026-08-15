namespace System.Collections.Generic;

/// <summary>
/// Represents a bidirectional dictionary that allows efficient lookups by both keys and values.
/// </summary>
/// <typeparam name="TKey">The type of the keys in the dictionary. Must be non-nullable.</typeparam>
/// <typeparam name="TValue">The type of the values in the dictionary. Must be non-nullable.</typeparam>
public class BiDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TKey : notnull where TValue : notnull
{
    private readonly Dictionary<TKey, TValue> _dic1;
    private readonly Dictionary<TValue, TKey> _dic2;

    public BiDictionary(IEqualityComparer<TKey>? keyComparer = null, IEqualityComparer<TValue>? valueComparer = null)
    {
        keyComparer ??= EqualityComparer<TKey>.Default;
        valueComparer ??= EqualityComparer<TValue>.Default;
        _dic1 = new(keyComparer);
        _dic2 = new(valueComparer);
    }

    public int Count => _dic1.Count;
    public ICollection<TKey> Keys => _dic1.Keys;
    public ICollection<TValue> Values => _dic2.Keys;
    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dic1.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) => _dic1.Contains(item);
    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((ICollection<KeyValuePair<TKey, TValue>>)_dic1).CopyTo(array, arrayIndex);
#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => _dic1.TryGetValue(key, out value);
#pragma warning restore CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
    public bool TryGetKey(TValue value, [MaybeNullWhen(false)] out TKey key) => _dic2.TryGetValue(value, out key);
    public bool ContainsKey(TKey key) => _dic1.ContainsKey(key);
    public bool ContainsValue(TValue value) => _dic2.ContainsKey(value);

    /// <summary>Gets the value associated with the specified key.</summary>
    public TValue GetValue(TKey key) => _dic1[key];

    /// <summary>Gets the key associated with the specified value.</summary>
    public TKey GetKey(TValue value) => _dic2[value];

    public void Clear()
    {
        _dic1.Clear();
        _dic2.Clear();
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
        ICollection<KeyValuePair<TKey, TValue>> col = _dic1;
        if (!col.Remove(item))
            return false;

        _dic2.Remove(item.Value);
        return true;
    }

    public void Add(TKey key, TValue value)
    {
        AddMapping(_dic1, _dic2, key, value);
    }

    public bool Remove(TKey key)
    {
        return RemoveKey(key);
    }

    public bool Remove(TValue value)
    {
        return RemoveValue(value);
    }

    /// <summary>Removes the mapping identified by its key.</summary>
    public bool RemoveKey(TKey key)
    {
        if (!_dic1.Remove(key, out var value))
            return false;

        _dic2.Remove(value);
        return true;
    }

    /// <summary>Removes the mapping identified by its value.</summary>
    public bool RemoveValue(TValue value)
    {
        if (!_dic2.Remove(value, out var key))
            return false;

        _dic1.Remove(key);
        return true;
    }

    public TValue this[TKey key]
    {
        get => GetValue(key);
        set => SetValue(key, value);
    }

    public TKey this[TValue lookupValue]
    {
        get => GetKey(lookupValue);
        set => SetKey(lookupValue, value);
    }

    /// <summary>Adds or replaces the mapping identified by its key.</summary>
    public void SetValue(TKey key, TValue value)
    {
        SetMapping(_dic1, _dic2, key, value, "An item with the same value has already been added.");
    }

    /// <summary>Adds or replaces the mapping identified by its value.</summary>
    public void SetKey(TValue value, TKey key)
    {
        SetMapping(_dic2, _dic1, value, key, "An item with the same key has already been added.");
    }

    private static void SetMapping<TForward, TReverse>(
        Dictionary<TForward, TReverse> forward,
        Dictionary<TReverse, TForward> reverse,
        TForward key,
        TReverse value,
        string duplicateMessage)
        where TForward : notnull
        where TReverse : notnull
    {
        if (!forward.TryGetValue(key, out var oldValue))
        {
            AddMapping(forward, reverse, key, value);
            return;
        }

        if (reverse.Comparer.Equals(oldValue, value))
            return;

        if (reverse.ContainsKey(value))
            throw new ArgumentException(duplicateMessage, nameof(value));

        reverse.Add(value, key);
        try
        {
            forward[key] = value;
        }
        catch
        {
            reverse.Remove(value);
            throw;
        }
        reverse.Remove(oldValue);
    }

    private static void AddMapping<TForward, TReverse>(
        Dictionary<TForward, TReverse> forward,
        Dictionary<TReverse, TForward> reverse,
        TForward key,
        TReverse value)
        where TForward : notnull
        where TReverse : notnull
    {
        forward.Add(key, value);
        try
        {
            reverse.Add(value, key);
        }
        catch
        {
            forward.Remove(key);
            throw;
        }
    }
}
