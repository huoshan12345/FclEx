namespace FclEx.Http;

/// <summary>
/// Convenience methods for common <see cref="IHttpService"/> operations such as simple GET requests, cookie management, and file downloads.
/// </summary>
public static class HttpServiceExtensions
{
    /// <summary>
    /// Sends a GET request to a URL string and returns the raw <see cref="HttpResponse"/>.
    /// The optional charset is used as the preferred response decoding charset.
    /// <paramref name="readHeadersTimeout"/> limits waiting for response headers, while <paramref name="totalTimeout"/> limits the whole request workflow.
    /// </summary>
    public static Task<HttpResponse> GetAsync(this IHttpService http, string url, string? charSet = null, TimeSpan? readHeadersTimeout = null, TimeSpan? totalTimeout = null)
    {
        return HttpRequest.Get(url)
            .ReadHeadersTimeout(readHeadersTimeout)
            .TotalTimeout(totalTimeout)
            .CharSet(charSet)
            .SendAsync(http);
    }

    /// <summary>
    /// Adds a cookie to the service cookie container, optionally scoped by the supplied URL.
    /// </summary>
    public static void AddCookie(this IHttpService http, Cookie cookie, string? url = null)
    {
        var uri = url == null ? null : new Uri(url);
        http.AddCookie(cookie, uri);
    }

    /// <summary>
    /// Gets a cookie by name for the specified URL.
    /// </summary>
    public static Cookie? GetCookie(this IHttpService http, string url, string name)
    {
        var uri = new Uri(url);
        return http.GetCookie(uri, name);
    }

    /// <summary>
    /// Gets all cookies that apply to the specified URL.
    /// </summary>
    public static IReadOnlyCollection<Cookie> GetCookies(this IHttpService http, string url)
    {
        var uri = new Uri(url);
        return http.GetCookies(uri);
    }

    /// <summary>
    /// Expires all cookies that apply to the specified URI.
    /// The cookies remain in the underlying container until the container removes expired entries.
    /// </summary>
    public static void ClearCookies(this IHttpService http, Uri uri)
    {
        foreach (var cookie in http.GetCookies(uri))
        {
            cookie.Expired = true;
        }
    }

    /// <summary>
    /// Expires all cookies that apply to the specified URL.
    /// </summary>
    public static void ClearCookies(this IHttpService http, string url)
    {
        var uri = new Uri(url);
        http.ClearCookies(uri);
    }

    /// <summary>
    /// Expires every cookie currently visible from the service cookie container.
    /// </summary>
    public static void ClearAllCookies(this IHttpService http)
    {
        foreach (var cookie in http.GetAllCookies())
        {
            cookie.Expired = true;
        }
    }

    /// <summary>
    /// Adds multiple cookies to the service cookie container, optionally scoped by the supplied URL.
    /// </summary>
    public static void AddCookies(this IHttpService http, IEnumerable<Cookie> cookies, string? url = null)
    {
        var uri = url == null ? null : new Uri(url);
        http.AddCookies(cookies, uri);
    }

    /// <summary>
    /// Adds multiple cookies to the service cookie container, optionally scoped by the supplied URI.
    /// </summary>
    public static void AddCookies(this IHttpService http, IEnumerable<Cookie> cookies, Uri? uri = null)
    {
        Check.NotNull(http);
        Check.NotNull(cookies);
        foreach (var cookie in cookies)
            http.AddCookie(cookie, uri);
    }

    /// <summary>
    /// Converts simple cookies to <see cref="Cookie"/> instances and adds them to the service cookie container.
    /// </summary>
    public static void AddCookies(this IHttpService http, IEnumerable<SimpleCookie> cookies, Uri? uri = null)
        => http.AddCookies(cookies.Select(m => m.ToCookie()), uri);

    /// <summary>
    /// Converts simple cookies to <see cref="Cookie"/> instances and adds them to the service cookie container, optionally scoped by URL.
    /// </summary>
    public static void AddCookies(this IHttpService http, IEnumerable<SimpleCookie> cookies, string? url)
    {
        var uri = url == null ? null : new Uri(url);
        http.AddCookies(cookies, uri);
    }

    /// <summary>
    /// Returns all cookies from the service cookie container as serializable <see cref="SimpleCookie"/> values.
    /// </summary>
    public static IReadOnlyList<SimpleCookie> GetAllSimpleCookies(this IHttpService http)
    {
        Check.NotNull(http);
        return http.GetAllCookies().Select(m => m.ToSimpleCookie()).ToList();
    }

    /// <summary>
    /// Converts a simple cookie to <see cref="Cookie"/> and adds it to the service cookie container.
    /// </summary>
    public static void AddCookie(this IHttpService http, SimpleCookie cookie)
    {
        Check.NotNull(http);
        Check.NotNull(cookie);
        http.AddCookie(cookie.ToCookie());
    }

    /// <summary>
    /// Adds every cookie in a <see cref="CookieCollection"/> to the service cookie container, optionally scoped by URL.
    /// </summary>
    public static void AddCookies(this IHttpService http, CookieCollection cc, string? url = null)
        => AddCookies(http, cc.OfType<Cookie>(), url);

    /// <summary>
    /// Downloads a file using a full option object and returns parsed file metadata plus response bytes.
    /// The helper reads the response body into memory, accepts compressed responses, and disposes <see cref="DownloadOptions.Content"/> after the request completes.
    /// </summary>
    public static async Task<OperationResult<HttpFileDownloadInfo>> DownloadAsync(this IHttpService http, DownloadOptions options)
    {
        var request = new HttpRequest(options.Uri, options.Method)
            .ReadAsBytes()
            .ReadHeadersTimeout(options.ReadHeadersTimeout)
            .ReadBufferTimeout(options.ReadBufferTimeout)
            .TotalTimeout(options.TotalTimeout)
            .BufferSize(options.BufferSize)
            .AcceptCompress();

        if (options.Content is { } content)
        {
            request.Content(content);
        }

        try
        {
            var response = await request.SendAsync(http, options.CancellationToken);
            return response.IsError
                ? Operation.ObjectError(response, response.Exception, response.Elapsed)
                    .Cast<HttpFileDownloadInfo>()
                : response.GetDownloadInfo(options.FileBaseName, options.FileExtension);
        }
        finally
        {
            options.Content?.Dispose();
        }
    }

    /// <summary>
    /// Downloads a URI into memory and returns file metadata plus response bytes.
    /// The request accepts compressed responses, reads the body as bytes, and keeps header and total timeouts separate.
    /// </summary>
    public static Task<OperationResult<HttpFileDownloadInfo>> DownloadAsync(this IHttpService http, Uri uri,
        HttpMethod? method = null, TimeSpan? readHeadersTimeout = null, TimeSpan? totalTimeout = null)
    {
        return http.DownloadAsync(new DownloadOptions
        {
            Uri = uri,
            Method = method ?? HttpMethod.Get,
            ReadHeadersTimeout = readHeadersTimeout,
            TotalTimeout = totalTimeout,
        });
    }

    /// <summary>
    /// Downloads a URL string into memory and returns file metadata plus response bytes.
    /// </summary>
    public static Task<OperationResult<HttpFileDownloadInfo>> DownloadAsync(this IHttpService http, string url,
        HttpMethod? method = null, TimeSpan? readHeadersTimeout = null, TimeSpan? totalTimeout = null)
    {
        return http.DownloadAsync(new Uri(url), method, readHeadersTimeout, totalTimeout);
    }

    /// <summary>
    /// Downloads multiple URL strings using the same batch options.
    /// Relative URL strings are resolved by <see cref="BatchDownloadOptions.BaseAddress"/> when one is provided.
    /// </summary>
    public static Task<OperationResult<HttpFileDownloadInfo>[]> BatchDownloadAsync(this IHttpService httpService, IEnumerable<string> uris, BatchDownloadOptions? options = null)
    {
        return httpService.BatchDownloadAsync(uris.Select(m => new Uri(m, UriKind.RelativeOrAbsolute)), options);
    }

    /// <summary>
    /// Downloads multiple URIs with optional shared method, content, timeouts, concurrency, and cancellation settings.
    /// When content is supplied, it is buffered once and cloned for each request so redirects or parallel sends do not reuse a consumed <see cref="HttpContent"/>.
    /// </summary>
    public static async Task<OperationResult<HttpFileDownloadInfo>[]> BatchDownloadAsync(this IHttpService httpService, IEnumerable<Uri> uris, BatchDownloadOptions? options = null)
    {
        var token = options?.CancellationToken ?? default;
        var maxDegreeOfParallelism = options?.MaxDegreeOfParallelism ?? BatchDownloadOptions.DefaultMaxDegreeOfParallelism;
        Check.Positive(maxDegreeOfParallelism);
        var readBufferTimeout = options?.ReadBufferTimeout ?? null;
        var bufferSize = options?.BufferSize ?? null;

        var sourceContent = options?.Content;
        var content = await sourceContent.ToBufferedContentAsync(readBufferTimeout, bufferSize, token);

        try
        {
            return await uris.SelectConcurrentlyAsync(
                (uri, operationToken) =>
                {
                    if (uri.IsAbsoluteUri == false && options?.BaseAddress is { } baseAddress)
                        uri = baseAddress.Resolve(uri);

                    return new ValueTask<OperationResult<HttpFileDownloadInfo>>(httpService.DownloadAsync(new DownloadOptions
                    {
                        Uri = uri,
                        Method = options?.Method ?? HttpMethod.Get,
                        Content = content?.Clone(),
                        ReadHeadersTimeout = options?.ReadHeadersTimeout,
                        BufferSize = bufferSize,
                        ReadBufferTimeout = options?.ReadBufferTimeout,
                        TotalTimeout = options?.TotalTimeout,
                        CancellationToken = operationToken,
                        FileBaseName = null,
                        FileExtension = null,
                    }));
                },
                maxDegreeOfParallelism,
                token);
        }
        finally
        {
            content?.Dispose();

            if (ReferenceEquals(sourceContent, content) == false)
                sourceContent?.Dispose();
        }
    }
}
