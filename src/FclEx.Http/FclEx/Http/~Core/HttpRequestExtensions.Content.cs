namespace FclEx.Http;

partial class HttpRequestExtensions
{
    /// <summary>
    /// Sets the request body content.
    /// The content is sent only for non-GET requests by the default service implementation.
    /// </summary>
    public static HttpRequest Content(this HttpRequest request, HttpContent? content)
    {
        request.Content = content;
        return request;
    }

    /// <summary>
    /// Sets the request body to string content encoded as UTF-8 unless another encoding is supplied.
    /// </summary>
    public static HttpRequest StringContent(this HttpRequest request, string data, Encoding? encoding = null)
    {
        return request.Content(new StringContent(data, encoding ?? Encoding.UTF8));
    }

    /// <summary>
    /// Sets the request body to a byte-array segment.
    /// </summary>
    public static HttpRequest ByteArrayContent(this HttpRequest request, byte[] data, int offset, int count)
    {
        request.Content = new ByteArrayContent(data, offset, count);
        return request;
    }

    /// <summary>
    /// Sets the request body to a whole byte array.
    /// </summary>
    public static HttpRequest ByteArrayContent(this HttpRequest request, byte[] data)
    {
        return request.ByteArrayContent(data, 0, data.Length);
    }

    /// <summary>
    /// Sets the request body to an array segment.
    /// A segment without an underlying array is treated as an empty byte array.
    /// </summary>
    public static HttpRequest ByteArrayContent(this HttpRequest request, ArraySegment<byte> data)
    {
        return request.ByteArrayContent(data.Array ?? [], data.Offset, data.Count);
    }

    /// <summary>
    /// Serializes <paramref name="data"/> using its runtime type and sets it as UTF-8 JSON request content.
    /// </summary>
    /// <param name="request">The request whose content will be replaced.</param>
    /// <param name="data">The value to serialize.</param>
    /// <param name="options">Serializer options, or <see langword="null"/> to use FclEx's default JSON options.</param>
    /// <returns>The same request, for chaining.</returns>
    /// <remarks>Unlike the generic overload, this overload selects the serialization type from <paramref name="data"/> at runtime.</remarks>
    /// <exception cref="JsonException">Serialization fails.</exception>
    /// <exception cref="NotSupportedException">No converter is available for the value's type.</exception>
    public static HttpRequest JsonContent(this HttpRequest request, object data, JsonSerializerOptions? options = null)
    {
        request.Content = HttpContent.Json(data, options);
        return request;
    }

    /// <summary>
    /// Serializes <paramref name="data"/> using its runtime type and the supplied JSON options, then sets UTF-8 JSON request content.
    /// </summary>
    /// <param name="request">The request whose content will be replaced.</param>
    /// <param name="data">The value to serialize.</param>
    /// <param name="options">The JSON options to convert to serializer options.</param>
    /// <returns>The same request, for chaining.</returns>
    /// <remarks>Unlike the generic overload, this overload selects the serialization type from <paramref name="data"/> at runtime.</remarks>
    /// <exception cref="JsonException">Serialization fails.</exception>
    /// <exception cref="NotSupportedException">No converter is available for the value's type.</exception>
    public static HttpRequest JsonContent(this HttpRequest request, object data, JsonOptions options)
    {
        return request.JsonContent(data, JsonHelper.GetOptions(options));
    }

    /// <summary>
    /// Serializes <paramref name="data"/> using its compile-time type and sets it as UTF-8 JSON request content.
    /// </summary>
    /// <typeparam name="T">The declared type used by <see cref="JsonSerializer"/> during serialization.</typeparam>
    /// <param name="request">The request whose content will be replaced.</param>
    /// <param name="data">The value to serialize.</param>
    /// <param name="options">Serializer options, or <see langword="null"/> to use FclEx's default JSON options.</param>
    /// <returns>The same request, for chaining.</returns>
    /// <remarks>
    /// Use the overload accepting <see cref="object"/> when serialization should use the value's runtime type.
    /// </remarks>
    /// <exception cref="JsonException">Serialization fails.</exception>
    /// <exception cref="NotSupportedException">No converter is available for the value's type.</exception>
    public static HttpRequest JsonContent<T>(this HttpRequest request, T data, JsonSerializerOptions? options = null)
    {
        request.Content = HttpContent.Json(data, options);
        return request;
    }

    /// <summary>
    /// Serializes <paramref name="data"/> using its compile-time type and the supplied JSON options, then sets UTF-8 JSON request content.
    /// </summary>
    /// <typeparam name="T">The declared type used by <see cref="JsonSerializer"/> during serialization.</typeparam>
    /// <param name="request">The request whose content will be replaced.</param>
    /// <param name="data">The value to serialize.</param>
    /// <param name="options">The JSON options to convert to serializer options.</param>
    /// <returns>The same request, for chaining.</returns>
    /// <remarks>Use the overload accepting <see cref="object"/> when serialization should use the value's runtime type.</remarks>
    /// <exception cref="JsonException">Serialization fails.</exception>
    /// <exception cref="NotSupportedException">No converter is available for the value's type.</exception>
    public static HttpRequest JsonContent<T>(this HttpRequest request, T data, JsonOptions options)
    {
        return request.JsonContent(data, JsonHelper.GetOptions(options));
    }

    /// <summary>
    /// Sets the request body to <see cref="FormUrlEncodedContent"/> built from the supplied name-value pairs.
    /// </summary>
    public static HttpRequest FormContent(this HttpRequest request, IEnumerable<KeyValuePair<string, string>> nameValueCollection)
    {
        request.Content = new FormUrlEncodedContent(nameValueCollection);
        return request;
    }

    /// <summary>
    /// Buffers the current request content so it can be reused for retries or redirects.
    /// The request's read-buffer timeout and buffer size are used while copying.
    /// </summary>
    public static Task<BufferedContent?> CreateBufferedContentAsync(this HttpRequest request, CancellationToken token = default)
    {
        return request.Content.ToBufferedContentAsync(request.ReadBufferTimeout, request.BufferSize, token);
    }
}
