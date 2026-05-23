namespace FclEx.Http;

partial class HttpRequestExtensions
{
    public static HttpRequest AddQueryParam(this HttpRequest request, string? key, string? value)
    {
        request.Query.Add(key, value);
        return request;
    }

    public static HttpRequest AddQueryParam<T>(this HttpRequest request, string? key, T? value)
    {
        request.Query.Add(key, value);
        return request;
    }

    public static HttpRequest AddQueryParam(this HttpRequest request, IEnumerable<KeyValuePair<string, string>> pairs)
    {
        request.Query.Add(pairs);
        return request;
    }

    public static HttpRequest AddQueryParam<T>(this HttpRequest request, T builder) where T : INameValuesBuilder
    {
        request.Query.Add(builder);
        return request;
    }

    public static HttpRequest AddQueryParam<T>(this HttpRequest request, IEnumerable<KeyValuePair<string, T>> pairs)
        where T : IEnumerable<string>
    {
        request.Query.Add(pairs);
        return request;
    }


    public static HttpRequest AddQueryValue(this HttpRequest request, string? value)
    {
        return request.AddQueryParam(null, value);
    }

}