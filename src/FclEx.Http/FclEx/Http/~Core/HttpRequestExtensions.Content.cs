namespace FclEx.Http;

partial class HttpRequestExtensions
{
    public static HttpRequest Content(this HttpRequest request, HttpContent content)
    {
        request.Content = content;
        return request;
    }

    public static HttpRequest Content(this HttpRequest request, string data, Encoding? encoding = null)
    {
        return request.Content(new StringContent(data, encoding ?? Encoding.UTF8));
    }

    public static HttpRequest Content(this HttpRequest request, byte[] data, int offset, int count)
    {
        request.Content = new ByteArrayContent(data, offset, count);
        return request;
    }

    public static HttpRequest Content(this HttpRequest request, byte[] data)
    {
        return request.Content(data, 0, data.Length);
    }

    public static HttpRequest Content(this HttpRequest request, ArraySegment<byte> data)
    {
        return request.Content(data.Array ?? [], data.Offset, data.Count);
    }

    public static HttpRequest JsonContent(this HttpRequest request, object data, JsonSerializerOptions? options = null)
    {
        request.Content = HttpContentHelper.ToJsonContent(data, options);
        return request;
    }

    public static HttpRequest FormContent(this HttpRequest request, IEnumerable<KeyValuePair<string, string>> nameValueCollection)
    {
        request.Content = new FormUrlEncodedContent(nameValueCollection);
        return request;
    }
}