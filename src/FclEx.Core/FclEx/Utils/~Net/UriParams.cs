namespace FclEx.Utils;

/// <summary>
/// Represents a collection of URL query parameters with encoding and rendering capabilities.
/// </summary>
/// <remarks>
/// UriParams specializes the NameValues base class to handle URL query string parameters.
/// It properly encodes parameter names and values according to URL encoding rules when rendering.<br/>
/// Unlike the base collection, UriParams uses case-sensitive string comparison (StringComparer.Ordinal)
/// by default, as URL query parameters are typically case-sensitive.<br/>
/// This class implements IRenderable to efficiently build query strings without unnecessary
/// string allocations when appending to an existing URL.
/// </remarks>
public sealed class UriParams() : NameValues<UriParams>(StringComparer.Ordinal), IRenderable
{
    /// <summary>
    /// Returns the URL-encoded query string representation of the parameters.
    /// </summary>
    /// <returns>A properly URL-encoded query string without the leading '?' character.</returns>
    public override string ToString()
    {
        return this.RenderToString();
    }

    /// <summary>
    /// Renders the URL-encoded query string to the provided StringBuilder.
    /// </summary>
    /// <param name="builder">The StringBuilder to append the encoded query parameters to.</param>
    /// <remarks>
    /// Parameters are rendered in key=value format, joined with '&amp;' characters.
    /// Both keys and values are properly URL-encoded using HttpUtility.UrlEncode.
    /// Empty keys will render just the value, and empty values will render just the key with '='.
    /// </remarks>
    public void Render(StringBuilder builder)
    {
        foreach (var (_, (key, value), _, isLast) in this.IndexEx())
        {
            // see https://source.dot.net/#System.Web.HttpUtility/System/Web/HttpUtility.cs,e8f7afaff17514d9,references
            if (key.IsNotEmpty())
            {
                builder.Append(HttpUtility.UrlEncode(key));
                builder.Append('=');
            }

            if (value.IsNotEmpty())
            {
                builder.Append(HttpUtility.UrlEncode(value));
            }

            if (isLast == false)
                builder.Append('&');
        }
    }

    /// <summary>
    /// Parses a query string into a UriParams collection.
    /// </summary>
    /// <param name="query">The query string to parse, with or without the leading '?' character.</param>
    /// <returns>A new UriParams instance containing the parsed parameters.</returns>
    /// <remarks>
    /// This method handles URL-decoding of parameter names and values.
    /// If the input is null or empty, an empty UriParams collection is returned.
    /// </remarks>
    public static UriParams Parse(string? query)
    {
        var dic = HttpUtility.ParseQueryString(query ?? "");
        return new UriParams().Add(dic.Enumerate());
    }

    /// <summary>
    /// Creates a new UriParams collection from a sequence of key-value pairs.
    /// </summary>
    /// <param name="pairs">The sequence of key-value pairs to add to the collection.</param>
    /// <returns>A new UriParams instance containing the provided parameters.</returns>
    public static UriParams From(IEnumerable<KeyValuePair<string, string>> pairs) => new UriParams().Add(pairs);

    /// <summary>
    /// Creates a new UriParams collection with a single key-value pair.
    /// </summary>
    /// <typeparam name="T">The type of the value, which will be converted to string.</typeparam>
    /// <param name="key">The parameter key.</param>
    /// <param name="value">The parameter value.</param>
    /// <returns>A new UriParams instance containing the provided parameter.</returns>
    public static UriParams From<T>(string? key, T value) => new UriParams().Add(key, value);
}