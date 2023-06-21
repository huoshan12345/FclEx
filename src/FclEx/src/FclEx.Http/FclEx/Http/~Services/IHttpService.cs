namespace FclEx.Http;

public interface IHttpService : IDisposable
{
    Task<HttpResponse> ExecuteAsync(HttpRequest request, CancellationToken token = default);

    void AddCookie(Cookie cookie, Uri? uri = null);

    Cookie? GetCookie(Uri uri, string name);

    IReadOnlyCollection<Cookie> GetCookies(Uri uri);

    IReadOnlyCollection<Cookie> GetAllCookies();

    IWebProxy? WebProxy { get; set; }

    ILogger Logger { get; set; }
}