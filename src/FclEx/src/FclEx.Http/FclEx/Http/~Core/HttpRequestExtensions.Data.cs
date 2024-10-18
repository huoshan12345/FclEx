using FclEx.Json;

namespace FclEx.Http;

public static partial class HttpRequestExtensions
{
    public static HttpRequest AddFormValue(this HttpRequest req, string key, string? value)
    {
        Check.NotNull(key);
        req.FormValues.Add(key.Trim(), value.ToStringOrEmpty().Trim());
        return req;
    }

    public static HttpRequest AddQueryValue<T>(this HttpRequest req, string key, T? value) => req.AddQueryValue(key, value.ToStringOrEmpty());

    public static HttpRequest AddQueryValue(this HttpRequest req, KeyValuePair<string, string?> pair) => req.AddQueryValue(pair.Key, pair.Value);

    public static HttpRequest AddQueryValue(this HttpRequest req, Tuple<string, string?> pair) => req.AddQueryValue(pair.Item1, pair.Item2);

    public static HttpRequest AddQueryValue(this HttpRequest req, (string, string?) pair) => req.AddQueryValue(pair.Item1, pair.Item2);

    public static HttpRequest AddQueryValue(this HttpRequest req, IEnumerable<KeyValuePair<string, string?>> paras)
    {
        paras.ForEach(m => req.AddQueryValue(m));
        return req;
    }

    public static HttpRequest AddQueryPair(this HttpRequest req, string queryPair, char separator = ':')
    {
        var pair = queryPair.Split(separator);
        return req.AddQueryValue(pair[0], pair.Length > 1 ? pair[1] : "");
    }

    public static HttpRequest AddFormValue<T>(this HttpRequest req, string key, T? value) => req.AddFormValue(key, value.ToStringOrEmpty());

    public static HttpRequest AddFormValue(this HttpRequest req, KeyValuePair<string, string?> pair) => req.AddFormValue(pair.Key, pair.Value);

    public static HttpRequest AddFormValue(this HttpRequest req, Tuple<string, string?> pair) => req.AddFormValue(pair.Item1, pair.Item2);

    public static HttpRequest AddFormValue(this HttpRequest req, (string, string?) pair) => req.AddFormValue(pair.Item1, pair.Item2);

    public static HttpRequest AddFormValue(this HttpRequest req, IEnumerable<KeyValuePair<string, string?>> paras)
    {
        paras?.ForEach(m => req.AddFormValue(m));
        return req;
    }

    public static HttpRequest AddFormPair(this HttpRequest req, string queryPair, char separator = ':')
    {
        var pair = queryPair.Split(separator);
        return req.AddFormValue(pair[0], pair.Length > 1 ? pair[1] : "");
    }

    public static HttpRequest AddDataIfNotEmpty(this HttpRequest req, string key, string? value)
    {
        return AddDataIf(req, !value.IsNullOrEmpty(), key, value);
    }

    public static HttpRequest AddDataIf(this HttpRequest req, bool condition, string key, string? value)
    {
        return condition ? AddData(req, key, value) : req;
    }

    public static HttpRequest AddData(this HttpRequest req, string key, string? value)
    {
        return req.Method == HttpMethod.Get
            ? req.AddQueryValue(key, value)
            : req.AddFormValue(key, value);
    }

    public static HttpRequest AddData<T>(this HttpRequest req, string key, T? value)
    {
        return AddData(req, key, value.ToStringOrEmpty());
    }

    public static HttpRequest AddData(this HttpRequest req, IEnumerable<KeyValuePair<string, string?>> paras)
    {
        return req.Method == HttpMethod.Get
            ? req.AddQueryValue(paras)
            : req.AddFormValue(paras);
    }

    public static HttpRequest AddDataPair(this HttpRequest req, string queryPair, char separator = ':')
    {
        return req.Method == HttpMethod.Get
            ? req.AddQueryPair(queryPair, separator)
            : req.AddFormPair(queryPair, separator);
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

    public static HttpRequest AddDataIfValid(this HttpRequest req, string key, string? value)
    {
        return req.AddDataIf(value.IsNotEmpty(), key, value!);
    }

    public static HttpRequest JsonContent(this HttpRequest req, object data, JsonOptions options = default)
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