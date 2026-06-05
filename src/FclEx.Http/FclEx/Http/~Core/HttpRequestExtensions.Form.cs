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
        request.Form.Add(key, value);
        return request;
    }

    public static HttpRequest AddFormParam(this HttpRequest request, IEnumerable<KeyValuePair<string, string>> pairs)
    {
        request.Form.Add(pairs);
        return request;
    }

    public static HttpRequest AddFormParam<T>(this HttpRequest request, T builder) where T : INameValuesBuilder
    {
        request.Form.Add(builder);
        return request;
    }

    public static HttpRequest AddFormParam<T>(this HttpRequest request, IEnumerable<KeyValuePair<string, T>> pairs)
        where T : IEnumerable<string>
    {
        request.Form.Add(pairs);
        return request;
    }
}