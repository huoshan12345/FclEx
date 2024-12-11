namespace FclEx.Http;

public static partial class HttpRequestExtensions
{
    public static HttpRequest AddQueryParam(this HttpRequest req, string key, string? value)
    {
        Check.NotNull(key);
        req.Query.Add(key, value);
        return req;
    }

    public static HttpRequest AddQueryParam<T>(this HttpRequest req, string key, T? value) => req.AddQueryParam(key, value.ToStringOrEmpty());

    public static HttpRequest AddQueryParam(this HttpRequest req, KeyValuePair<string, string?> pair) => req.AddQueryParam(pair.Key, pair.Value);

    public static HttpRequest AddQueryParam(this HttpRequest req, Tuple<string, string?> pair) => req.AddQueryParam(pair.Item1, pair.Item2);

    public static HttpRequest AddQueryParam(this HttpRequest req, (string, string?) pair) => req.AddQueryParam(pair.Item1, pair.Item2);

    public static HttpRequest AddQueryParam(this HttpRequest req, IEnumerable<KeyValuePair<string, string?>> paras)
    {
        paras.ForEach(m => req.AddQueryParam(m));
        return req;
    }

    public static HttpRequest AddQueryPair(this HttpRequest req, string queryPair, char separator = ':')
    {
        var pair = queryPair.Split(separator);
        return req.AddQueryParam(pair[0], pair.Length > 1 ? pair[1] : "");
    }

    public static HttpRequest AddQueryParam(this HttpRequest request, IEnumerable<UriParam> enumerable)
    {
        foreach (var (key, value) in enumerable)
        {
            request.AddQueryParam(key, value);
        }
        return request;
    }

    public static HttpRequest AddQueryValue<T>(this HttpRequest request, T builder) where T : IUriParamsBuilder
    {
        return request.AddQueryParam(builder.Build());
    }

    public static HttpRequest AddFormParam(this HttpRequest req, string key, string? value)
    {
        Check.NotNull(key);
        req.Form.Add(key.Trim(), value.ToStringOrEmpty().Trim());
        return req;
    }

    public static HttpRequest AddFormParam<T>(this HttpRequest req, string key, T? value) => req.AddFormParam(key, value.ToStringOrEmpty());

    public static HttpRequest AddFormParam(this HttpRequest req, KeyValuePair<string, string?> pair) => req.AddFormParam(pair.Key, pair.Value);

    public static HttpRequest AddFormParam(this HttpRequest req, Tuple<string, string?> pair) => req.AddFormParam(pair.Item1, pair.Item2);

    public static HttpRequest AddFormParam(this HttpRequest req, (string, string?) pair) => req.AddFormParam(pair.Item1, pair.Item2);

    public static HttpRequest AddFormParam(this HttpRequest req, IEnumerable<KeyValuePair<string, string?>> paras)
    {
        paras?.ForEach(m => req.AddFormParam(m));
        return req;
    }

    public static HttpRequest AddFormPair(this HttpRequest req, string queryPair, char separator = ':')
    {
        var pair = queryPair.Split(separator);
        return req.AddFormParam(pair[0], pair.Length > 1 ? pair[1] : "");
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

    public static HttpRequest Content(this HttpRequest req, HttpContent content)
    {
        req.Content = content;
        return req;
    }

    public static HttpRequest Content(this HttpRequest req, string data, Encoding? encoding = null)
    {
        return req.Content(new StringContent(data, encoding ?? Encoding.UTF8));
    }

    public static HttpRequest Content(this HttpRequest req, byte[] data, int offset, int count)
    {
        req.Content = new ByteArrayContent(data, offset, count);
        return req;
    }

    public static HttpRequest Content(this HttpRequest req, byte[] data)
    {
        return req.Content(data, 0, data.Length);
    }

    public static HttpRequest Content(this HttpRequest req, ArraySegment<byte> data)
    {
        return req.Content(data.Array ?? Array.Empty<byte>(), data.Offset, data.Count);
    }

    public static HttpRequest JsonContent(this HttpRequest req, object data, JsonSerializerOptions? options = null)
    {
        req.Content = HttpContentHelper.ToJsonContent(data, options);
        return req;
    }

    public static HttpRequest FormContent(this HttpRequest req, IEnumerable<KeyValuePair<string, string>> nameValueCollection)
    {
        req.Content = new FormUrlEncodedContent(nameValueCollection);
        return req;
    }
}