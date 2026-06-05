namespace FclEx.Http;

partial class HttpRequestExtensions
{
    public static HttpRequest Content(this HttpRequest request, HttpContent? content)
    {
        request.Content = content;
        return request;
    }

    public static HttpRequest StringContent(this HttpRequest request, string data, Encoding? encoding = null)
    {
        return request.Content(new StringContent(data, encoding ?? Encoding.UTF8));
    }

    public static HttpRequest ByteArrayContent(this HttpRequest request, byte[] data, int offset, int count)
    {
        request.Content = new ByteArrayContent(data, offset, count);
        return request;
    }

    public static HttpRequest ByteArrayContent(this HttpRequest request, byte[] data)
    {
        return request.ByteArrayContent(data, 0, data.Length);
    }

    public static HttpRequest ByteArrayContent(this HttpRequest request, ArraySegment<byte> data)
    {
        return request.ByteArrayContent(data.Array ?? [], data.Offset, data.Count);
    }

    public static HttpRequest JsonContent(this HttpRequest request, object data, JsonSerializerOptions? options = null)
    {
        request.Content = HttpContentHelper.ToJsonContent(data, options);
        return request;
    }

    public static HttpRequest JsonContent(this HttpRequest request, object data, JsonOptions options)
    {
        return request.JsonContent(data, JsonHelper.GetOptions(options));
    }

    public static HttpRequest FormContent(this HttpRequest request, IEnumerable<KeyValuePair<string, string>> nameValueCollection)
    {
        request.Content = new FormUrlEncodedContent(nameValueCollection);
        return request;
    }

    public static Task<BufferedContent?> CreateBufferedContentAsync(this HttpRequest request, CancellationToken token = default)
    {
        return request.Content.ToBufferedContentAsync(request.ReadBufferTimeout, request.BufferSize, token);
    }
}