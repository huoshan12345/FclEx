namespace FclEx.Http;

public static partial class HttpRequestExtensions
{
    public static Task<HttpResponse> SendAsync(this HttpRequest request, IHttpService? service = null)
    {
        return (service ?? HttpClientService.Default).SendAsync(request);
    }

    public static Task<HttpResponse> SendAsync(this HttpRequest request, IHttpService service, IAsyncPolicy policy)
    {
        return policy.ExecuteAsync(() => request.SendAsync(service));
    }

    public static string Dump(this HttpRequest request, IEnumerable<Cookie> cookies)
    {
        var disposable = StringBuilderHelper.GetCached();
        var builder = disposable.Value;

        builder.Append(request.Method);
        builder.Append(' ');
        var uri = request.GetUri();
        builder.AppendLine(uri.ToString());

        foreach (var (key, values) in request.Headers)
        {
            foreach (var value in values)
            {
                builder.Append(key);
                builder.Append(": ");
                builder.AppendLine(value);
            }
        }

        var cookieStr = cookies.Select(m => m.ToString()).JoinWith("; ");
        if (cookieStr.IsNotEmpty())
        {
            builder.Append(HttpHeaderNames.Cookie);
            builder.Append(": ");
            builder.AppendLine(cookieStr);
        }

        return builder.ToString();
    }

    public static string Dump(this HttpRequest request, IHttpService service)
    {
        return request.Dump(service.GetCookies(request.GetUri()));
    }
}