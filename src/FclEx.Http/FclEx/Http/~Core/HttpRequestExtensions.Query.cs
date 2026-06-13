namespace FclEx.Http;

partial class HttpRequestExtensions
{
    /// <summary>
    /// Adds a query parameter to the request URI builder.
    /// Existing values for the same key are preserved.
    /// </summary>
    public static HttpRequest AddQueryParam(this HttpRequest request, string? key, string? value)
    {
        request.Query.Add(key, value);
        return request;
    }

    /// <summary>
    /// Adds a query parameter after converting the value through <see cref="UriParams"/>.
    /// </summary>
    public static HttpRequest AddQueryParam<T>(this HttpRequest request, string? key, T? value)
    {
        request.Query.Add(key, value);
        return request;
    }

    /// <summary>
    /// Adds multiple query parameters to the request URI builder.
    /// </summary>
    public static HttpRequest AddQueryParam(this HttpRequest request, IEnumerable<KeyValuePair<string, string>> pairs)
    {
        request.Query.Add(pairs);
        return request;
    }

    /// <summary>
    /// Adds query parameters produced by a name-values builder.
    /// </summary>
    public static HttpRequest AddQueryParam<T>(this HttpRequest request, T builder) where T : INameValuesBuilder
    {
        request.Query.Add(builder);
        return request;
    }

    /// <summary>
    /// Adds multiple query parameters with multiple values per key.
    /// </summary>
    public static HttpRequest AddQueryParam<T>(this HttpRequest request, IEnumerable<KeyValuePair<string, T>> pairs)
        where T : IEnumerable<string>
    {
        request.Query.Add(pairs);
        return request;
    }


    /// <summary>
    /// Adds an unnamed query value to the request URI builder.
    /// </summary>
    public static HttpRequest AddQueryValue(this HttpRequest request, string? value)
    {
        return request.AddQueryParam(null, value);
    }

}
