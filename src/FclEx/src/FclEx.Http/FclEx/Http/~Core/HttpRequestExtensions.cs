namespace FclEx.Http;

public static partial class HttpRequestExtensions
{
    public static Task<HttpResponse> SendAsync(this HttpRequest req, IHttpService? service = null, int retryTimes = 0, int delaySeconds = 0)
    {
        return (service ?? HttpClientService.Default).SendAsync(req, retryTimes, delaySeconds);
    }

    public static Task<HttpResponse> SendAsync(this HttpRequest req, IHttpService service, IAsyncPolicy policy)
    {
        return policy.ExecuteAsync(() => req.SendAsync(service));
    }
}