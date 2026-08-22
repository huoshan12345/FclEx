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
    /// Serializes an object to JSON and sets it as UTF-8 JSON request content.
    /// </summary>
    public static HttpRequest JsonContent(this HttpRequest request, object data, JsonSerializerOptions? options = null)
    {
        request.Content = HttpContent.Json(data, options);
        return request;
    }

    /// <summary>
    /// Serializes an object to JSON with named JSON options and sets it as UTF-8 JSON request content.
    /// </summary>
    public static HttpRequest JsonContent(this HttpRequest request, object data, JsonOptions options)
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
