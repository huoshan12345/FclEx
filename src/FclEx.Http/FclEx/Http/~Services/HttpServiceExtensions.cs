namespace FclEx.Http;

public static class HttpServiceExtensions
{
    public static Task<HttpResponse> GetAsync(this IHttpService http, string url, string? charSet = null, int? timeoutMilliseconds = 10 * 1000)
    {
        return HttpRequest.Get(url)
            .TryReadHeadersTimeout(timeoutMilliseconds is { } t ? TimeSpan.FromMilliseconds(t) : null)
            .CharSet(charSet)
            .SendAsync(http);
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

    public static async Task<OperationResult<HttpFileDownloadInfo>> DownloadAsync(this IHttpService http, Uri uri, HttpMethod? method = null, TimeSpan? timeout = null)
    {
        var request = new HttpRequest(uri, method ?? HttpMethod.Get)
            .ReadAsBytes()
            .ReadBufferTimeout(timeout)
            .AcceptCompress();

        var response = await request.SendAsync(http);
        return response.IsError
            ? Operation.ObjectError(response, response.Exception!, response.Elapsed)
                .Cast<HttpFileDownloadInfo>()
            : response.GetDownloadInfo();
    }

    public static Task<OperationResult<HttpFileDownloadInfo>> DownloadAsync(this IHttpService http, string url, HttpMethod? method = null, TimeSpan? timeout = null)
        => http.DownloadAsync(new Uri(url), method, timeout);

    public static Task<OperationResult<HttpFileDownloadInfo>[]> BatchDownloadAsync(this IHttpService httpService, IEnumerable<string> uris, BatchDownloadOptions? options = null)
    {
        return httpService.BatchDownloadAsync(uris.Select(m => new Uri(m, UriKind.RelativeOrAbsolute)), options);
    }

    public static async Task<OperationResult<HttpFileDownloadInfo>[]> BatchDownloadAsync(this IHttpService httpService, IEnumerable<Uri> uris, BatchDownloadOptions? options = null)
    {
        var token = options?.CancellationToken ?? default;
        var readBufferTimeout = options?.ReadBufferTimeout ?? null;
        var bufferSize = options?.BufferSize ?? null;
        var disposeContent = options?.DisposeContent ?? true;

        var content = await (options?.Content).ToBufferedContentAsync(readBufferTimeout, bufferSize, token);

        try
        {
            return await uris.ExecuteAsync(uri =>
            {
                if (uri.IsAbsoluteUri == false && options?.BaseAddress is { } baseAddress)
                {
                    uri = baseAddress.Resolve(uri);
                }
                return httpService.DownloadAsync(new DownloadOptions
                {
                    Uri = uri,
                    Method = options?.Method ?? HttpMethod.Get,
                    Content = content?.Clone(),
                    ReadHeadersTimeout = options?.ReadHeadersTimeout,
                    BufferSize = bufferSize,
                    ReadBufferTimeout = options?.ReadBufferTimeout,
                    CancellationToken = token,
                    FileBaseName = null,
                    FileExtension = null,
                    DisposeContent = disposeContent,
                });
            }, options?.ExecuteInParallel ?? true, options?.Concurrency, TimeSpan.Zero, token);
        }
        finally
        {
            if (disposeContent)
                content?.Dispose();
        }
    }
}