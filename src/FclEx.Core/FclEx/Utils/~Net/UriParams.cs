namespace FclEx.Utils;

/// <summary>
/// Represents a collection of URI parameters as key-value pairs, 
/// supporting operations for adding, setting, and rendering parameters.
/// </summary>
/// <remarks>
/// This class allows managing URI query parameters with the flexibility to handle duplicates 
/// using the <see cref="Add"/> method or enforce uniqueness using the <see cref="Set"/> method.
/// It implements <see cref="IReadOnlyCollection{T}"/> for read-only enumeration of parameters 
/// and <see cref="IRenderable"/> for custom rendering functionality.
/// </remarks>
public sealed class UriParams : IReadOnlyCollection<UriParam>, IRenderable
{
    // there is mirror bug in HttpQSCollection https://github.com/dotnet/runtime/issues/71871
    // so we use MultiValueDictionary as the entries. 
    private readonly MultiValueDictionary<string, string> _entries;
    private int? _count; // null count means that is not calculated yet.

    public UriParams(string? query = null)
    {
        var dic = HttpUtility.ParseQueryString(query ?? "");
        _entries = dic.Enumerate().ToMultiValueDictionary();
    }

    public UriParams(IEnumerable<UriParam> parameters)
    {
        _entries = parameters.ToMultiValueDictionary(m => m.Key, m => m.Value);
    }

    public UriParams(IEnumerable<KeyValuePair<string?, string?>> parameters)
    {
        _entries = parameters.ToMultiValueDictionary(m => m.Key ?? "", m => m.Value ?? "");
    }

    public UriParams(string? key, string? value)
    {
        _entries = [];
        Add(key ?? "", value ?? "");
    }

    public override string ToString()
    {
        return this.RenderToString();
    }

    public void Render(StringBuilder builder)
    {
        foreach (var (_, item, _, isLast) in this.IndexEx())
        {
            item.Render(builder);

            if (isLast == false)
                builder.Append('&');
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<UriParam> GetEnumerator()
    {
        return _entries.SelectMany(m => m.Value, (m, n) => new UriParam(m.Key, n)).GetEnumerator();
    }

    /// <summary>
    /// Adds a key-value pair to the URI parameters. 
    /// Duplicate values for the same key are allowed and will be preserved.
    /// </summary>
    /// <param name="key">The key to add. Must not be null or empty.</param>
    /// <param name="value">The value to associate with the key. If null, an empty string is used.</param>
    /// <returns>The current <see cref="UriParams"/> instance, allowing for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="key"/> is null or empty.</exception>
    public UriParams Add(string? key, object? value)
    {
        _entries.Add(key ?? "", value?.ToString() ?? "");
        _count = null;
        return this;
    }

    /// <summary>
    /// Sets a key-value pair in the URI parameters. 
    /// If the key already exists, any existing values are removed before adding the new value.
    /// Duplicate values for the same key are not allowed.
    /// </summary>
    /// <param name="key">The key to set. Must not be null or empty.</param>
    /// <param name="value">The value to associate with the key. If null, an empty string is used.</param>
    /// <returns>The current <see cref="UriParams"/> instance, allowing for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="key"/> is null or empty.</exception>
    public UriParams Set(string? key, object? value)
    {
        key ??= "";
        _entries.Remove(key);
        _count = null;
        return Add(key, value);
    }

    /// <summary>
    /// Removes all entries with the specified key from the collection.
    /// </summary>
    /// <param name="key">The key to remove. Must not be null or empty.</param>
    /// <returns>The current <see cref="UriParams"/> instance, allowing for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="key"/> is null or empty.</exception>
    public UriParams Remove(string? key)
    {
        _entries.Remove(key ?? "");
        _count = null;
        return this;
    }

    /// <summary>
    /// Retrieves the latest value associated with the specified key, if it exists.
    /// </summary>
    /// <param name="key">The key to look up. Must not be null or empty.</param>
    /// <returns>
    /// The latest value associated with the key, or <c>null</c> if the key does not exist.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="key"/> is null or empty.</exception>
    public string? Get(string? key)
    {
        return GetValues(key).LastOrDefault();
    }

    public string? this[string? key]
    {
        get => Get(key);
        set => Set(key, value);
    }

    /// <summary>
    /// Retrieves all values associated with the specified key.
    /// </summary>
    /// <param name="key">The key to look up. Must not be null or empty.</param>
    /// <returns>
    /// A read-only collection of all values associated with the key. 
    /// If the key does not exist, returns an empty collection.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="key"/> is null or empty.</exception>
    public IReadOnlyCollection<string> GetValues(string? key)
    {
        return _entries.Get(key ?? "");
    }

    /// <summary>
    /// Attempts to retrieve the latest value associated with the specified key.
    /// </summary>
    /// <param name="key">The key to look up. Must not be null or empty.</param>
    /// <param name="value">
    /// When this method returns, contains the latest value associated with the key, 
    /// or <c>null</c> if the key does not exist.
    /// </param>
    /// <returns>
    /// <c>true</c> if the key exists and has at least one associated value; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="key"/> is null or empty.</exception>
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
    /// Attempts to retrieve all values associated with the specified key.
    /// </summary>
    /// <param name="key">The key to look up. Must not be null or empty.</param>
    /// <param name="values">
    /// When this method returns, contains a read-only collection of all values associated with the key, 
    /// or <c>null</c> if the key does not exist.
    /// </param>
    /// <returns>
    /// <c>true</c> if the key exists and has at least one associated value; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="key"/> is null or empty.</exception>
    public bool TryGetValues(string? key, [NotNullWhen(true)] out IReadOnlyCollection<string>? values)
    {
        return _entries.TryGetValue(key ?? "", out values);
    }

    public int Count => _count ??= _entries.Sum(m => m.Value.Count);

    public void Clear()
    {
        _entries.Clear();
        _count = 0;
    }

    public IReadOnlyCollection<string> Keys => _entries.Keys;

    public static UriParams Parse(string? query) => new(query);

    public static UriParams From(IEnumerable<KeyValuePair<string?, string?>> pairs) => new(pairs);
    public static UriParams From(string? key, string? value) => new(key, value);
    public static UriParams From(string? key, object? value) => new(key, value?.ToString());
}