namespace FclEx.Http;

/// <summary>
/// Fluent helpers for sending, inspecting, and converting <see cref="HttpRequest"/> instances.
/// </summary>
public static partial class HttpRequestExtensions
{
    /// <summary>
    /// Sends a request through the supplied service, or through <see cref="HttpClientService.Default"/> when no service is supplied.
    /// </summary>
    public static Task<HttpResponse> SendAsync(
        this HttpRequest request,
        IHttpService? service = null,
        CancellationToken token = default)
    {
        return (service ?? HttpClientService.Default).SendAsync(request, token);
    }

    /// <summary>
    /// Sends a request through a temporary <see cref="HttpClientService"/> created from a client provider.
    /// The created service follows <paramref name="disposeHttpClient"/> when it disposes the provided client.
    /// </summary>
    public static Task<HttpResponse> SendAsync(
        this HttpRequest request,
        Func<HttpClient> httpClientProvider,
        bool disposeHttpClient = true,
        CancellationToken token = default)
    {
        return HttpClientService.Create(httpClientProvider, disposeHttpClient).SendAsync(request, token);
    }

    /// <summary>
    /// Executes request sending through an external Polly policy.
    /// The policy receives the caller's cancellation token.
    /// </summary>
    public static Task<HttpResponse> SendAsync(
        this HttpRequest request,
        IHttpService service,
        IAsyncPolicy policy,
        CancellationToken token = default)
    {
        return policy.ExecuteAsync(t => request.SendAsync(service, t), token);
    }

    /// <summary>
    /// Formats the request line, request headers, and supplied cookies for trace logging.
    /// The request body is not included.
    /// </summary>
    public static string Dump(this HttpRequest request, IEnumerable<Cookie> cookies)
    {
        using var disposable = StringBuilder.GetCached();
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

    /// <summary>
    /// Formats the request for trace logging and includes cookies from the service when the request URI is absolute.
    /// </summary>
    public static string Dump(this HttpRequest request, IHttpService service)
    {
        var uri = request.GetUri();
        return request.Dump(uri.IsAbsoluteUri ? service.GetCookies(uri) : []);
    }
    
    /// <summary>
    /// Wraps an HTTP request as an executable action.
    /// </summary>
    /// <param name="request">The request to send.</param>
    /// <param name="httpService">The service used to send the request. Uses the default service when <see langword="null"/>.</param>
    /// <param name="unwrapError">Whether a failed <see cref="HttpResponse"/> should become an error result containing that response.</param>
    /// <returns>An action that sends <paramref name="request"/> and returns the response.</returns>
    public static IAction<HttpResponse> ToAction(this HttpRequest request, IHttpService? httpService = null, bool unwrapError = true)
    {
        return new HttpRequestAction(request, httpService ?? HttpClientService.Default, unwrapError);
    }
}
