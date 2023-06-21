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
}