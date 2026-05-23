namespace FclEx.Http;

public interface IHttpService : IDisposable
{
    Task<HttpResponse> SendAsync(HttpRequest request, CancellationToken token = default);

    void AddCookie(Cookie cookie, Uri? uri = null, bool overrideDomain = false);

    Cookie? GetCookie(Uri uri, string name);

    IReadOnlyCollection<Cookie> GetCookies(Uri uri);

    IReadOnlyCollection<Cookie> GetAllCookies();

    IWebProxy? Proxy { get; set; }

    ILogger Logger { get; set; }
}