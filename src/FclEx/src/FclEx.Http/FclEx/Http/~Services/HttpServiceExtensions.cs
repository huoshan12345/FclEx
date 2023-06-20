namespace FclEx.Http;

public static class HttpServiceExtensions
{
    public static Task<HttpResponse> GetAsync(this IHttpService http, string url, string? charSet = null, int? timeout = 10 * 1000, int retryTimes = 3, int delaySeconds = 0)
    {
        var req = HttpRequest.Get(url)
            .TryConnectTimeout(timeout == null ? null : TimeSpan.FromMilliseconds(timeout.Value))
            .CharSet(charSet);
        return http.SendAsync(req, retryTimes, delaySeconds);
    }

    public static async Task<HttpResponse> SendAsync(this IHttpService http, HttpRequest req, int retryTimes = 1, int delaySeconds = 0)
    {

        return await ActionHelper.TryAsync(() => http.ExecuteAsync(req),
                retryTimes, delaySeconds, e => HttpResponse.CreateError(req, e), false, HttpResponse.EmptyRes)
            .DonotCapture();
    }

    public static void AddCookie(this IHttpService http, Cookie cookie, string? url = null)
    {
        var uri = url == null ? null : new Uri(url);
        http.AddCookie(cookie, uri);
    }

    public static Cookie? GetCookie(this IHttpService http, string url, string name)
    {
        var uri = new Uri(url);
        return http.GetCookie(uri, name);
    }

    public static IReadOnlyCollection<Cookie> GetCookies(this IHttpService http, string url)
    {
        var uri = new Uri(url);
        return http.GetCookies(uri);
    }

    public static void ClearCookies(this IHttpService http, Uri uri)
    {
        foreach (var cookie in http.GetCookies(uri))
        {
            cookie.Expired = true;
        }
    }

    public static void ClearCookies(this IHttpService http, string url)
    {
        var uri = new Uri(url);
        http.ClearCookies(uri);
    }

    public static void ClearAllCookies(this IHttpService http)
    {
        foreach (var cookie in http.GetAllCookies())
        {
            cookie.Expired = true;
        }
    }

    public static void AddCookies(this IHttpService http, IEnumerable<Cookie> cookies, string? url = null)
    {
        var uri = url == null ? null : new Uri(url);
        http.AddCookies(cookies, uri);
    }

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public static void AddCookies(this IHttpService http, IEnumerable<Cookie> cookies, Uri? uri = null)
    {
        Check.NotNull(http);
        Check.NotNull(cookies);
        foreach (var cookie in cookies)
            http.AddCookie(cookie, uri);
    }

    public static void AddCookies(this IHttpService http, IEnumerable<SimpleCookie> cookies, Uri? uri = null)
        => http.AddCookies(cookies.Select(m => m.ToCookie()), uri);

    public static void AddCookies(this IHttpService http, IEnumerable<SimpleCookie> cookies, string? url)
    {
        var uri = url == null ? null : new Uri(url);
        http.AddCookies(cookies, uri);
    }

    public static IReadOnlyList<SimpleCookie> GetAllSimpleCookies(this IHttpService http)
    {
        Check.NotNull(http);
        return http.GetAllCookies().Select(m => m.ToSimpleCookie()).ToList();
    }

    public static void AddCookie(this IHttpService http, SimpleCookie cookie)
    {
        Check.NotNull(http);
        Check.NotNull(cookie);
        http.AddCookie(cookie.ToCookie());
    }

    public static void AddCookies(this IHttpService http, CookieCollection cc, string? url = null)
        => AddCookies(http, cc.OfType<Cookie>(), url);

    public static async Task<OperateResult<HttpFileDownloadInfo>> DownloadAsync(this IHttpService http, Uri uri, HttpMethod? method = null, TimeSpan? timeout = null)
    {
        var request = new HttpRequest(uri, method ?? HttpMethod.Get)
            .ResponseType(HttpResponseType.Stream)
            .ReadBufferTimeout(timeout)
            .AcceptCompress();

        var res = await http.SendAsync(request).DonotCapture();
        if (res.HasError)
            return Operate
                .CreateObjError(res, res.Exception!, res.ExecuteTime)
                .ToExplicit<HttpFileDownloadInfo>();
        else
            return res.GetDownloadInfo();
    }

    public static Task<OperateResult<HttpFileDownloadInfo>> DownloadAsync(this IHttpService http, string url, HttpMethod? method = null, TimeSpan? timeout = null)
        => http.DownloadAsync(new Uri(url), method, timeout);
}