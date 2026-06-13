namespace FclEx.Http;

/// <summary>
/// Stores HTTP headers with case-insensitive keys and repeated values.
/// </summary>
/// <remarks>
/// Empty header names are ignored. Setting a header value to <see langword="null"/> removes the header.
/// </remarks>
public class HttpHeaders() : NameValues<HttpHeaders>(StringComparer.OrdinalIgnoreCase), IRenderable
{
    /// <summary>
    /// Writes headers using the HTTP header-line format.
    /// </summary>
    /// <param name="builder">The builder to append to.</param>
    public void Render(StringBuilder builder)
    {
        foreach (var (key, value) in this)
        {
            builder.Append(key);
            builder.Append(": ");
            builder.Append(value);
            builder.Append("\r\n");
        }
    }

    /// <summary>
    /// Adds a header value when the header name is not null or empty.
    /// </summary>
    /// <param name="key">The header name.</param>
    /// <param name="value">The header value.</param>
    /// <returns>The same header collection.</returns>
    public override HttpHeaders Add(string? key, string? value)
    {
        // http headers do not allow empty key
        if (key.IsNullOrEmpty())
            return this;

        return base.Add(key, value);
    }

    /// <summary>
    /// Replaces a header value, or removes the header when <paramref name="value"/> is null.
    /// </summary>
    /// <param name="key">The header name.</param>
    /// <param name="value">The replacement value, or <see langword="null"/> to remove the header.</param>
    /// <returns>The same header collection.</returns>
    public override HttpHeaders Set(string? key, string? value)
    {
        // http headers do not allow empty key
        if (key.IsNullOrEmpty())
            return this;

        // use null to remove header
        return value == null
            ? Remove(key)
            : base.Set(key, value);
    }

    /// <summary>
    /// Parses headers from a query-string-style representation.
    /// </summary>
    /// <param name="query">The encoded name-value string. <see langword="null"/> is treated as empty.</param>
    /// <returns>A header collection containing the parsed values.</returns>
    public static HttpHeaders Parse(string? query)
    {
        var dic = UriParams.Parse(query ?? "");
        return new HttpHeaders().Add(dic);
    }

    /// <summary>
    /// Creates headers from name-value pairs.
    /// </summary>
    /// <param name="pairs">The header pairs to add.</param>
    /// <returns>A header collection containing the supplied pairs.</returns>
    public static HttpHeaders From(IEnumerable<KeyValuePair<string, string>> pairs) => new HttpHeaders().Add(pairs);

    /// <summary>
    /// Creates headers with one value converted to a string by the underlying name-value collection.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="key">The header name.</param>
    /// <param name="value">The value to add.</param>
    /// <returns>A header collection containing the supplied value.</returns>
    public static HttpHeaders From<T>(string? key, T value) => new HttpHeaders().Add(key, value);
}
