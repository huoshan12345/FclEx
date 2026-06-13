namespace FclEx.Http;

/// <summary>
/// Sends <see cref="HttpRequest"/> instances and exposes shared cookie, proxy, and logging state.
/// </summary>
public interface IHttpService : IDisposable
{
    /// <summary>
    /// Sends a request and returns an <see cref="HttpResponse"/> that represents the transport outcome.
    /// </summary>
    /// <param name="request">The request to send.</param>
    /// <param name="token">A cancellation token for the send operation.</param>
    /// <returns>The response object. Implementations may store non-cancellation failures in <see cref="HttpResponse.Exception"/>.</returns>
    Task<HttpResponse> SendAsync(HttpRequest request, CancellationToken token = default);

    /// <summary>
    /// Adds a cookie to the service cookie store.
    /// </summary>
    /// <param name="cookie">The cookie to add.</param>
    /// <param name="uri">The URI used for domain validation and storage. When null, the cookie's own domain is used.</param>
    /// <param name="overrideDomain">Whether to replace the cookie domain with <paramref name="uri"/>'s host before storing.</param>
    void AddCookie(Cookie cookie, Uri? uri = null, bool overrideDomain = false);

    /// <summary>
    /// Gets a cookie by URI and name from the service cookie store.
    /// </summary>
    Cookie? GetCookie(Uri uri, string name);

    /// <summary>
    /// Gets cookies for one URI from the service cookie store.
    /// </summary>
    IReadOnlyCollection<Cookie> GetCookies(Uri uri);

    /// <summary>
    /// Gets every cookie currently stored by the service.
    /// </summary>
    IReadOnlyCollection<Cookie> GetAllCookies();

    /// <summary>
    /// The proxy used by newly created HTTP clients or requests.
    /// </summary>
    IWebProxy? Proxy { get; set; }

    /// <summary>
    /// The logger used by the service.
    /// </summary>
    ILogger Logger { get; set; }
}
