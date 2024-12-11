using Microsoft.Collections.Extensions;

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
    private readonly MultiValueDictionary<string, string> _entries;

    public UriParams(string? query = null)
    {
        var dic = HttpUtility.ParseQueryString(query ?? "");
        _entries = dic.Enumerate().ToMultiValueDictionary();
    }

    public UriParams(IEnumerable<UriParam> parameters)
    {
        _entries = parameters.ToMultiValueDictionary(m => m.Key, m => m.Value);
    }

    public UriParams(IEnumerable<KeyValuePair<string, string>> parameters)
    {
        _entries = parameters.ToMultiValueDictionary(m => m.Key, m => m.Value);
    }

    public override string ToString()
    {
        return this.RenderToString();
    }

    public void Render(StringBuilder builder)
    {
        foreach (var (key, values) in _entries)
        {
            // don't support empty key
            if (key.IsNullOrEmpty())
                continue;

            foreach (var value in values)
            {
                builder.Append(HttpUtility.UrlEncode(key));
                builder.Append('=');
                if (value.IsNotEmpty())
                {
                    builder.Append(HttpUtility.UrlEncode(value));
                }
                builder.Append('&');
            }
        }

        if (builder.Length > 0)
            builder.Length--; // remove the trailing extra &
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
    public UriParams Add(string key, string? value)
    {
        Check.NotEmpty(key);
        _entries.Add(key, value ?? "");
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
    public UriParams Set(string key, string? value)
    {
        Check.NotEmpty(key);
        _entries.Remove(key);
        return Add(key, value);
    }

    /// <summary>
    /// Removes all entries with the specified key from the collection.
    /// </summary>
    /// <param name="key">The key to remove. Must not be null or empty.</param>
    /// <returns>The current <see cref="UriParams"/> instance, allowing for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="key"/> is null or empty.</exception>
    public UriParams Remove(string key)
    {
        Check.NotEmpty(key);
        _entries.Remove(key);
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
    public string? Get(string key)
    {
        return GetValues(key).LastOrDefault();
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
    public IReadOnlyCollection<string> GetValues(string key)
    {
        Check.NotEmpty(key);
        return _entries.Get(key);
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
    public bool TryGet(string key, [NotNullWhen(true)] out string? value)
    {
        Check.NotEmpty(key);

        if (TryGetValues(key, out var values))
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
    public bool TryGetValues(string key, [NotNullWhen(true)] out IReadOnlyCollection<string>? values)
    {
        return _entries.TryGetValue(key, out values);
    }

    public int Count => _entries.Count;

    public static UriParams Parse(string? query) => new(query);

    public static UriParams From(IEnumerable<KeyValuePair<string, string>> pairs) => new(pairs);
}

public static class UriParameterCollectionExtensions
{
    public static IEnumerable<KeyValuePair<string, string>> AsKeyValuePairs(this UriParams collection)
    {
        return collection.Select(m => m.ToKeyValuePair());
    }

    public static UriParams Add(this UriParams collection, IEnumerable<KeyValuePair<string, string>> parameters)
    {
        Check.NotNull(parameters);
        foreach (var (key, value) in parameters)
        {
            collection.Add(key, value);
        }
        return collection;
    }

    public static UriParams Add(this UriParams collection, UriParam parameter)
    {
        return collection.Add(parameter.Key, parameter.Value);
    }
}