namespace FclEx.Http;

public static partial class HttpRequestExtensions
{
    public static Task<HttpResponse> SendAsync(
        this HttpRequest request,
        IHttpService? service = null,
        CancellationToken token = default)
    {
        return (service ?? HttpClientService.Default).SendAsync(request, token);
    }

    public static Task<HttpResponse> SendAsync(
        this HttpRequest request,
        Func<HttpClient> httpClientProvider,
        bool disposeHttpClient = true,
        CancellationToken token = default)
    {
        return HttpClientService.Create(httpClientProvider, disposeHttpClient).SendAsync(request, token);
    }

    public static Task<HttpResponse> SendAsync(
        this HttpRequest request,
        IHttpService service,
        IAsyncPolicy policy,
        CancellationToken token = default)
    {
        return policy.ExecuteAsync(() => request.SendAsync(service, token));
    }

    public static string Dump(this HttpRequest request, IEnumerable<Cookie> cookies)
    {
        var disposable = StringBuilderHelper.GetCached();
        var builder = disposable.Value;

        builder.Append(request.Method);
        builder.Append(' ');
        var uri = request.GetUri();
        builder.AppendLine(uri.ToString());

        foreach (var (key, value) in request.Headers)
        {
            builder.Append(key);
            builder.Append(": ");
            builder.AppendLine(value);
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