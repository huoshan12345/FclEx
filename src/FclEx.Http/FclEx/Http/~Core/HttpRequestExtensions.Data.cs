namespace FclEx.Http;

public static partial class HttpRequestExtensions
{
    public static HttpRequest AddQueryParam(this HttpRequest request, string key, string? value)
    {
        Check.NotNull(key);
        request.Query.Add(key, value);
        return request;
    }

    public static HttpRequest AddQueryParam<T>(this HttpRequest request, string key, T? value) => request.AddQueryParam(key, value.ToStringOrEmpty());

    public static HttpRequest AddQueryParam(this HttpRequest request, KeyValuePair<string, string?> pair) => request.AddQueryParam(pair.Key, pair.Value);

    public static HttpRequest AddQueryParam(this HttpRequest request, Tuple<string, string?> pair) => request.AddQueryParam(pair.Item1, pair.Item2);

    public static HttpRequest AddQueryParam(this HttpRequest request, (string, string?) pair) => request.AddQueryParam(pair.Item1, pair.Item2);

    public static HttpRequest AddQueryParam(this HttpRequest request, IEnumerable<KeyValuePair<string, string?>> paras)
    {
        paras.ForEach(m => request.AddQueryParam(m));
        return request;
    }

    public static HttpRequest AddQueryPair(this HttpRequest request, string queryPair, char separator = ':')
    {
        var pair = queryPair.Split(separator);
        return request.AddQueryParam(pair[0], pair.Length > 1 ? pair[1] : "");
    }

    public static HttpRequest AddQueryParam(this HttpRequest request, IEnumerable<UriParam> enumerable)
    {
        foreach (var (key, value) in enumerable)
        {
            request.AddQueryParam(key, value);
        }
        return request;
    }

    public static HttpRequest AddQueryParam<T>(this HttpRequest request, T builder) where T : IUriParamsBuilder
    {
        return request.AddQueryParam(builder.Build());
    }

    public static HttpRequest AddFormParam(this HttpRequest request, string key, string? value)
    {
        Check.NotNull(key);
        request.Form.Add(key, value);
        return request;
    }

    public static HttpRequest AddFormParam<T>(this HttpRequest request, string key, T? value) => request.AddFormParam(key, value.ToStringOrEmpty());

    public static HttpRequest AddFormParam(this HttpRequest request, KeyValuePair<string, string?> pair) => request.AddFormParam(pair.Key, pair.Value);

    public static HttpRequest AddFormParam(this HttpRequest request, Tuple<string, string?> pair) => request.AddFormParam(pair.Item1, pair.Item2);

    public static HttpRequest AddFormParam(this HttpRequest request, (string, string?) pair) => request.AddFormParam(pair.Item1, pair.Item2);

    public static HttpRequest AddFormParam(this HttpRequest request, IEnumerable<KeyValuePair<string, string?>> paras)
    {
        paras?.ForEach(m => request.AddFormParam(m));
        return request;
    }

    public static HttpRequest AddFormPair(this HttpRequest request, string queryPair, char separator = ':')
    {
        var pair = queryPair.Split(separator);
        return request.AddFormParam(pair[0], pair.Length > 1 ? pair[1] : "");
    }

    public static HttpRequest AddFormParam(this HttpRequest request, IEnumerable<UriParam> enumerable)
    {
        foreach (var (key, value) in enumerable)
        {
            request.AddFormParam(key, value);
        }
        return request;
    }

    public static HttpRequest AddFormParam<T>(this HttpRequest request, T builder) where T : IUriParamsBuilder
    {
        return request.AddFormParam(builder.Build());
    }

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