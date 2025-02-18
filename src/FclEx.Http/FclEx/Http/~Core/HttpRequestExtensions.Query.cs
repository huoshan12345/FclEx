namespace FclEx.Http;

partial class HttpRequestExtensions
{
    public static HttpRequest AddQueryParam(this HttpRequest request, string? key, string? value)
    {
        request.Query.Add(key, value);
        return request;
    }

    public static HttpRequest AddQueryValue(this HttpRequest request, string? value)
    {
        return request.AddQueryParam(null, value);
    }

    public static HttpRequest AddQueryParam<T>(this HttpRequest request, string? key, T? value)
    {
        return request.AddQueryParam(key, value?.ToString());
    }

    public static HttpRequest AddQueryParam(this HttpRequest request, KeyValuePair<string?, string?> pair)
    {
        return request.AddQueryParam(pair.Key, pair.Value);
    }

    public static HttpRequest AddQueryParam(this HttpRequest request, Tuple<string?, string?> pair)
    {
        return request.AddQueryParam(pair.Item1, pair.Item2);
    }

    public static HttpRequest AddQueryParam(this HttpRequest request, (string?, string?) pair)
    {
        return request.AddQueryParam(pair.Item1, pair.Item2);
    }

    public static HttpRequest AddQueryParam(this HttpRequest request, IEnumerable<KeyValuePair<string?, string?>> enumerable)
    {
        foreach (var (key, value) in enumerable.EmptyIfNull())
        {
            request.AddQueryParam(key, value);
        }
        return request;
    }

    public static HttpRequest AddQueryPair(this HttpRequest request, string queryPair, char separator = ':')
    {
        Check.NotNull(queryPair);
        var pair = queryPair.Split(separator);
        return request.AddQueryParam(pair[0], pair.Length > 1 ? pair[1] : "");
    }

    public static HttpRequest AddQueryParam(this HttpRequest request, IEnumerable<UriParam> enumerable)
    {
        foreach (var (key, value) in enumerable.EmptyIfNull())
        {
            request.AddQueryParam(key, value);
        }
        return request;
    }

    public static HttpRequest AddQueryParam<T>(this HttpRequest request, T builder) where T : IUriParamsBuilder
    {
        return request.AddQueryParam(builder.Build());
    }

    // To fix the nullable warning when paras' type is IEnumerable<KeyValuePair<string, string?>>
    public static void AddQueryParam(this HttpRequest request, IDictionary<string, string?> paras)
    {
        foreach (var (key, value) in paras)
        {
            request.AddQueryParam(key, value);
        }
    }
}