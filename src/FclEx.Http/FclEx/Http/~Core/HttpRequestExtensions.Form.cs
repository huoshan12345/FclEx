namespace FclEx.Http;

partial class HttpRequestExtensions
{
    public static HttpRequest AddFormParam(this HttpRequest request, string? key, string? value)
    {
        request.Form.Add(key, value);
        return request;
    }

    public static HttpRequest AddFormParam<T>(this HttpRequest request, string? key, T? value)
    {
        return request.AddFormParam(key, value?.ToString());
    }

    public static HttpRequest AddFormParam(this HttpRequest request, KeyValuePair<string?, string?> pair)
    {
        return request.AddFormParam(pair.Key, pair.Value);
    }

    public static HttpRequest AddFormParam(this HttpRequest request, Tuple<string?, string?> pair)
    {
        return request.AddFormParam(pair.Item1, pair.Item2);
    }

    public static HttpRequest AddFormParam(this HttpRequest request, (string?, string?) pair)
    {
        return request.AddFormParam(pair.Item1, pair.Item2);
    }

    public static HttpRequest AddFormParam(this HttpRequest request, IEnumerable<KeyValuePair<string?, string?>> enumerable)
    {
        foreach (var (key, value) in enumerable.EmptyIfNull())
        {
            request.AddFormParam(key, value);
        }
        return request;
    }

    public static HttpRequest AddFormPair(this HttpRequest request, string queryPair, char separator = ':')
    {
        Check.NotNull(queryPair);
        var pair = queryPair.Split(separator);
        return request.AddFormParam(pair[0], pair.Length > 1 ? pair[1] : "");
    }

    public static HttpRequest AddFormParam(this HttpRequest request, IEnumerable<UriParam> enumerable)
    {
        foreach (var (key, value) in enumerable.EmptyIfNull())
        {
            request.AddFormParam(key, value);
        }
        return request;
    }

    public static HttpRequest AddFormParam<T>(this HttpRequest request, T builder) where T : IUriParamsBuilder
    {
        return request.AddFormParam(builder.Build());
    }

    // To fix the nullable warning when paras' type is IEnumerable<KeyValuePair<string, string?>>
    public static HttpRequest AddFormParam<T>(this HttpRequest request, IDictionary<string, T> paras)
    {
        foreach (var (key, value) in paras)
        {
            request.AddFormParam(key, value);
        }
        return request;
    }
}