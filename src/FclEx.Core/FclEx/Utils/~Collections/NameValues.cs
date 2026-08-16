namespace FclEx.Utils;

/// <summary>
/// Represents a collection of key-value pairs where keys can have multiple values.
/// This generic class serves as a base for derived collection types with self-referencing generics pattern.
/// </summary>
/// <typeparam name="TSelf">The derived type implementing this collection for fluent method chaining</typeparam>
public abstract class NameValues<TSelf> : IReadOnlyCollection<KeyValuePair<string, string>> where TSelf : NameValues<TSelf>
{
    /// <summary>
    /// The underlying dictionary storing multiple values per key
    /// </summary>
    protected readonly MultiValueDictionary<string, string> _entries;

    /// <summary>
    /// Total count of key-value pairs in the collection
    /// </summary>
    protected int _count;

    /// <summary>
    /// Tracks modifications to the collection for enumeration safety
    /// </summary>
    protected int _version;

    /// <summary>
    /// Initializes a new instance of the NameValues collection
    /// </summary>
    /// <param name="keyComparer">The string comparer to use for key equality</param>
    protected NameValues(StringComparer keyComparer)
    {
        _entries = new(keyComparer);
    }

    /// <summary>
    /// Returns an enumerator that iterates through all key-value pairs in the collection
    /// </summary>
    /// <returns>An enumerator for the collection</returns>
    /// <exception cref="InvalidOperationException">Thrown if collection is modified during enumeration</exception>
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        var version = _version;
        foreach (var (key, values) in _entries)
        {
            foreach (var value in values)
            {
                if (version != _version)
                    throw new InvalidOperationException(Strings.InvalidOperation_EnumFailedVersion);

                yield return KeyValuePair.Create(key, value);
            }
        }
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection
    /// </summary>
    /// <returns>An enumerator for the collection</returns>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Gets the total number of key-value pairs in the collection
    /// </summary>
    public int Count => _count;

    /// <summary>
    /// Adds a key-value pair to the collection
    /// </summary>
    /// <param name="key">The key (null values become empty strings)</param>
    /// <param name="value">The value (null values become empty strings)</param>
    /// <returns>The instance for method chaining</returns>
    public virtual TSelf Add(string? key, string? value)
    {
        _entries.Add(key ?? "", value ?? "");
        _count++;
        _version++;
        return (TSelf)this;
    }

    /// <summary>
    /// Removes all existing values for the key and adds a new key-value pair
    /// </summary>
    /// <param name="key">The key (null values become empty strings)</param>
    /// <param name="value">The value (null values become empty strings)</param>
    /// <returns>The instance for method chaining</returns>
    public virtual TSelf Set(string? key, string? value)
    {
        Remove(key);
        return Add(key, value);
    }

    /// <summary>
    /// Removes all key-value pairs with the specified key
    /// </summary>
    /// <param name="key">The key to remove (null values become empty strings)</param>
    /// <returns>The instance for method chaining</returns>
    public virtual TSelf Remove(string? key)
    {
        if (_entries.Remove(key ?? "", out var values))
        {
            _count -= values.Count;
            _version++;
        }
        return (TSelf)this;
    }

    /// <summary>
    /// Gets the last value associated with the specified key.
    /// </summary>
    /// <remarks>
    /// This method returns only the last value instead of concatenating all values (like <see cref="NameValueCollection"/>) 
    /// to avoid ambiguity when values themselves contain delimiters like commas.<br/>
    /// If you need all values for a key, use <see cref="GetValues"/> method instead.
    /// To create a custom delimiter-separated string, use GetValues and implement your own joining logic with
    /// a delimiter appropriate for your data.
    /// </remarks>
    /// <param name="key">The key to look up</param>
    /// <returns>The last value for the key, or null if the key doesn't exist</returns>
    public string? Get(string? key)
    {
        return GetValues(key)?.LastOrDefault();
    }

    /// <summary>
    /// Gets or sets the value for the specified key
    /// When getting, returns the last value for the key
    /// When setting, removes all existing values for the key and adds the new value
    /// </summary>
    /// <param name="key">The key to access</param>
    /// <returns>The last value for the key, or null if the key doesn't exist</returns>
    public string? this[string? key]
    {
        get => Get(key);
        set => Set(key, value);
    }

    /// <summary>
    /// Gets all values associated with the specified key
    /// </summary>
    /// <param name="key">The key to look up</param>
    /// <returns>A collection of values for the key, or null if the key doesn't exist</returns>
    public IReadOnlyCollection<string>? GetValues(string? key)
    {
        return _entries.Get(key ?? "", null);
    }

    /// <summary>
    /// Attempts to get the last value associated with the specified key
    /// </summary>
    /// <param name="key">The key to look up</param>
    /// <param name="value">When this method returns, contains the last value for the key if found, or null if not found</param>
    /// <returns>true if the key was found; otherwise, false</returns>
    public bool TryGet(string? key, [NotNullWhen(true)] out string? value)
    {
        if (TryGetValues(key ?? "", out var values))
        {
            value = values.Last();
            return true;
        }

        value = null;
        return false;
    }

    /// <summary>
    /// Checks if the dictionary contains an entry with the specified key.
    /// If the provided key is null, it is treated as an empty string.
    /// </summary>
    /// <param name="key">The key to check for in the dictionary.</param>
    /// <returns>True if the dictionary contains the key; otherwise, false.</returns>
    public bool ContainsKey(string? key)
    {
        return _entries.ContainsKey(key ?? "");
    }

    /// <summary>
    /// Attempts to get all values associated with the specified key
    /// </summary>
    /// <param name="key">The key to look up</param>
    /// <param name="values">When this method returns, contains the collection of values if the key was found; otherwise, null</param>
    /// <returns>true if the key was found; otherwise, false</returns>
    public bool TryGetValues(string? key, [NotNullWhen(true)] out IReadOnlyCollection<string>? values)
    {
        return _entries.TryGetValue(key ?? "", out values);
    }

    /// <summary>
    /// Removes all key-value pairs from the collection
    /// </summary>
    /// <returns>The instance for method chaining</returns>
    public TSelf Clear()
    {
        _version++;
        _entries.Clear();
        _count = 0;
        return (TSelf)this;
    }

    /// <summary>
    /// Gets a collection containing all the keys in the collection.
    /// </summary>
    /// <remarks>
    /// This property returns only the distinct keys, regardless of how many values are associated with each key.
    /// The order of keys is not guaranteed to be consistent between calls.
    /// </remarks>
    public IReadOnlyCollection<string> Keys => _entries.Keys;
}

/// <summary>
/// A concrete implementation of the NameValues collection.
/// This non-generic class inherits from the generic base class with itself as the type parameter
/// to create a self-referencing generic type for fluent method chaining.
/// </summary>
public class NameValues : NameValues<NameValues>
{
    /// <summary>
    /// Initializes a new instance of the NameValues collection
    /// </summary>
    /// <param name="keyComparer">The string comparer to use for key equality</param>
    public NameValues(StringComparer keyComparer)
        : base(keyComparer)
    {
    }
}