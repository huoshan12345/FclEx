namespace FclEx.Http;

partial class HttpRequestExtensions
{
    /// <summary>
    /// Adds a form parameter that will be sent as form-url-encoded content when no explicit request content is set.
    /// Existing values for the same key are preserved.
    /// </summary>
    public static HttpRequest AddFormParam(this HttpRequest request, string? key, string? value)
    {
        request.Form.Add(key, value);
        return request;
    }

    /// <summary>
    /// Adds a form parameter after converting the value through <see cref="UriParams"/>.
    /// </summary>
    public static HttpRequest AddFormParam<T>(this HttpRequest request, string? key, T? value)
    {
        request.Form.Add(key, value);
        return request;
    }

    /// <summary>
    /// Adds multiple form parameters that will be used to create <see cref="FormUrlEncodedContent"/>.
    /// </summary>
    public static HttpRequest AddFormParam(this HttpRequest request, IEnumerable<KeyValuePair<string, string>> pairs)
    {
        request.Form.Add(pairs);
        return request;
    }

    /// <summary>
    /// Adds form parameters produced by a name-values builder.
    /// </summary>
    public static HttpRequest AddFormParam<T>(this HttpRequest request, T builder) where T : INameValuesBuilder
    {
        request.Form.Add(builder);
        return request;
    }

    /// <summary>
    /// Adds multiple form parameters with multiple values per key.
    /// </summary>
    public static HttpRequest AddFormParam<T>(this HttpRequest request, IEnumerable<KeyValuePair<string, T>> pairs)
        where T : IEnumerable<string>
    {
        request.Form.Add(pairs);
        return request;
    }
}
