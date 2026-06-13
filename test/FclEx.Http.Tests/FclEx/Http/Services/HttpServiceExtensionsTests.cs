namespace FclEx.Http.Services;

public class HttpServiceExtensionsTests
{
    [Fact]
    public async Task GetAsync_BuildsGetRequestWithCharsetAndReadHeadersTimeout()
    {
        var service = new CaptureRequestHttpService();

        await service.GetAsync("https://example.com/api", "gb2312", TimeSpan.FromMilliseconds(1234));

        Assert.NotNull(service.Request);
        Assert.Equal(HttpMethod.Get, service.Request.Method);
        Assert.Equal(new Uri("https://example.com/api"), service.Request.GetUri());
        Assert.Equal("gb2312", service.Request.CharSet);
        Assert.Equal(TimeSpan.FromMilliseconds(1234), service.Request.ReadHeadersTimeout);
    }

    [Fact]
    public async Task GetAsync_WhenTimeoutIsNull_LeavesDefaultReadHeadersTimeout()
    {
        var service = new CaptureRequestHttpService();

        await service.GetAsync("https://example.com/api", readHeadersTimeout: null);

        Assert.NotNull(service.Request);
        Assert.Null(service.Request.ReadHeadersTimeout);
    }

    [Fact]
    public void AddCookie_WithStringUrl_ConvertsUrlToUri()
    {
        var service = new CaptureRequestHttpService();
        var cookie = new Cookie("sid", "abc");

        service.AddCookie(cookie, "https://example.com/account");

        var call = Assert.Single(service.AddCookieCalls);
        Assert.Same(cookie, call.Cookie);
        Assert.Equal(new Uri("https://example.com/account"), call.Uri);
        Assert.False(call.OverrideDomain);
    }

    [Fact]
    public void AddCookie_WhenUrlIsNull_DelegatesWithNullUri()
    {
        var service = new CaptureRequestHttpService();
        var cookie = new Cookie("sid", "abc");

        service.AddCookie(cookie);

        var call = Assert.Single(service.AddCookieCalls);
        Assert.Same(cookie, call.Cookie);
        Assert.Null(call.Uri);
        Assert.False(call.OverrideDomain);
    }

    [Fact]
    public void AddCookie_WithSimpleCookie_ConvertsAndAddsCookie()
    {
        var service = new CaptureRequestHttpService();
        var cookie = new SimpleCookie("sid", "abc", "/account", "example.com");

        service.AddCookie(cookie);

        var call = Assert.Single(service.AddCookieCalls);
        Assert.Equal("sid", call.Cookie.Name);
        Assert.Equal("abc", call.Cookie.Value);
        Assert.Equal("/account", call.Cookie.Path);
        Assert.Equal("example.com", call.Cookie.Domain);
        Assert.Null(call.Uri);
    }

    [Fact]
    public void AddCookies_WithCookieEnumerable_AddsEachCookieWithSameUri()
    {
        var service = new CaptureRequestHttpService();
        var uri = new Uri("https://example.com/account");
        var cookies = new[]
        {
            new Cookie("one", "1"),
            new Cookie("two", "2"),
        };

        service.AddCookies(cookies, uri);

        Assert.Equal(["one", "two"], service.AddCookieCalls.Select(m => m.Cookie.Name));
        Assert.All(service.AddCookieCalls, call => Assert.Equal(uri, call.Uri));
    }

    [Fact]
    public void AddCookies_WhenHttpServiceIsNull_ThrowsArgumentNullException()
    {
        IHttpService service = null!;

        var ex = Assert.Throws<ArgumentNullException>(() =>
            service.AddCookies([new Cookie("sid", "abc")], (Uri?)null));

        Assert.Equal("http", ex.ParamName);
    }

    [Fact]
    public void AddCookies_WhenCookiesAreNull_ThrowsArgumentNullException()
    {
        var service = new CaptureRequestHttpService();
        IEnumerable<Cookie> cookies = null!;

        var ex = Assert.Throws<ArgumentNullException>(() =>
            service.AddCookies(cookies, (Uri?)null));

        Assert.Equal("cookies", ex.ParamName);
    }

    [Fact]
    public void AddCookies_WithSimpleCookiesAndStringUrl_AddsEachCookieWithUri()
    {
        var service = new CaptureRequestHttpService();
        SimpleCookie[] cookies =
        [
            new("one", "1", "/", "example.com"),
            new("two", "2", "/", "example.com"),
        ];

        service.AddCookies(cookies, "https://example.com/account");

        Assert.Equal(["one", "two"], service.AddCookieCalls.Select(m => m.Cookie.Name));
        Assert.All(service.AddCookieCalls, call => Assert.Equal(new Uri("https://example.com/account"), call.Uri));
    }

    [Fact]
    public void AddCookies_WithCookieCollectionAndStringUrl_AddsEachCookieWithUri()
    {
        var service = new CaptureRequestHttpService();
        var collection = new CookieCollection();
        collection.Add(new Cookie("one", "1"));
        collection.Add(new Cookie("two", "2"));

        service.AddCookies(collection, "https://example.com/");

        Assert.Equal(2, service.AddCookieCalls.Count);
        Assert.Equal(["one", "two"], service.AddCookieCalls.Select(m => m.Cookie.Name));
        Assert.All(service.AddCookieCalls, m => Assert.Equal(new Uri("https://example.com/"), m.Uri));
    }

    [Fact]
    public void GetCookie_WithStringUrl_ConvertsUrlAndDelegatesLookup()
    {
        var service = new CaptureRequestHttpService();
        var cookie = new Cookie("sid", "abc", "/", "example.com");
        service.CookiesByUri[new Uri("https://example.com/")] = [cookie];

        var actual = service.GetCookie("https://example.com/", "sid");

        Assert.Same(cookie, actual);
        Assert.Equal(new Uri("https://example.com/"), service.LastGetCookieUri);
        Assert.Equal("sid", service.LastGetCookieName);
    }

    [Fact]
    public void GetCookies_WithStringUrl_ConvertsUrlAndDelegatesLookup()
    {
        var service = new CaptureRequestHttpService();
        var cookie = new Cookie("sid", "abc", "/", "example.com");
        service.CookiesByUri[new Uri("https://example.com/")] = [cookie];

        var cookies = service.GetCookies("https://example.com/");

        Assert.Equal([cookie], cookies);
        Assert.Equal(new Uri("https://example.com/"), service.LastGetCookiesUri);
    }

    [Fact]
    public void GetAllSimpleCookies_ConvertsAllCookies()
    {
        var service = new CaptureRequestHttpService();
        service.AllCookies.Add(new Cookie("sid", "abc", "/account", "example.com"));

        var cookies = service.GetAllSimpleCookies();

        var cookie = Assert.Single(cookies);
        Assert.Equal("sid", cookie.Name);
        Assert.Equal("abc", cookie.Value);
        Assert.Equal("/account", cookie.Path);
        Assert.Equal("example.com", cookie.Domain);
    }

    [Fact]
    public void GetAllSimpleCookies_WhenHttpServiceIsNull_ThrowsArgumentNullException()
    {
        IHttpService service = null!;

        var ex = Assert.Throws<ArgumentNullException>(() => service.GetAllSimpleCookies());

        Assert.Equal("http", ex.ParamName);
    }

    [Fact]
    public void ClearCookies_MarksCookiesForUriAsExpired()
    {
        var service = new CaptureRequestHttpService();
        var uri = new Uri("https://example.com/account");
        var cookies = new[]
        {
            new Cookie("one", "1", "/", "example.com"),
            new Cookie("two", "2", "/", "example.com"),
        };
        service.CookiesByUri[uri] = cookies;

        service.ClearCookies(uri);

        Assert.All(cookies, cookie => Assert.True(cookie.Expired));
    }

    [Fact]
    public void ClearAllCookies_MarksAllCookiesAsExpired()
    {
        var service = new CaptureRequestHttpService();
        service.AllCookies.AddRange([
            new Cookie("one", "1", "/", "example.com"),
            new Cookie("two", "2", "/", "example.com"),
        ]);

        service.ClearAllCookies();

        Assert.All(service.AllCookies, cookie => Assert.True(cookie.Expired));
    }

    [RetryTheory]
    [InlineData("https://www.google.com/", "www_google_com.html")]
    [InlineData("https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-9.0/covariant-returns", "covariant-returns.html")]
    [InlineData("https://devblogs.microsoft.com/dotnet/csharp-exploring-extension-members/#comments", "csharp-exploring-extension-members.html")]
    public async Task DownloadAsync_WithStringUri_DerivesFileNameFromResolvedResponse(string uri, string fileName)
    {
        using var http = new HttpClientService();

        var (success, file, exception, _) = await http.DownloadAsync(uri);

        Assert.True(success, () => exception!.ToString());
        Assert.NotNull(file);
        Assert.Equal(fileName, file.FileName);
        Assert.Equal(Path.GetExtension(fileName), file.FileExtension);
        Assert.Equal(Path.GetFileNameWithoutExtension(fileName), file.FileNameWithoutExtension);
    }

    [Fact]
    public async Task DownloadAsync_MapsReadTimeoutOptionsToRequest()
    {
        var service = new CaptureRequestHttpService();
        var readHeadersTimeout = TimeSpan.FromSeconds(3);
        var readBufferTimeout = TimeSpan.FromSeconds(5);

        var result = await service.DownloadAsync(new DownloadOptions
        {
            Uri = new Uri("https://example.com/file.txt"),
            ReadHeadersTimeout = readHeadersTimeout,
            ReadBufferTimeout = readBufferTimeout,
        });

        Assert.True(result.IsError);
        Assert.NotNull(service.Request);
        Assert.Equal(readHeadersTimeout, service.Request.ReadHeadersTimeout);
        Assert.Equal(readBufferTimeout, service.Request.ReadBufferTimeout);
    }

    [Fact]
    public async Task DownloadAsync_WhenResponseSucceeds_ReturnsDownloadInfoFromResponse()
    {
        var service = new CaptureRequestHttpService
        {
            ResponseFactory = request =>
            {
                var response = new HttpResponse(request)
                {
                    StatusCode = HttpStatusCode.OK,
                    ResponseBytes = Encoding.UTF8.GetBytes("file"),
                };
                response.Headers.Add(HttpHeaderNames.ContentType, "text/plain");
                response.VisitedUris.Add(request.GetUri());
                return response;
            },
        };

        var result = await service.DownloadAsync(new Uri("https://example.com/files/report.txt"), HttpMethod.Head, TimeSpan.FromSeconds(7));

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.NotNull(service.Request);
        Assert.Equal(HttpMethod.Head, service.Request.Method);
        Assert.Equal(HttpContentType.Bytes, service.Request.ResponseContentType);
        Assert.Equal(TimeSpan.FromSeconds(7), service.Request.ReadBufferTimeout);
#if NET5_0_OR_GREATER
        Assert.Equal("gzip, deflate, br", service.Request.Headers.Get(HttpHeaderNames.AcceptEncoding));
#else
        Assert.Equal("gzip", service.Request.Headers.Get(HttpHeaderNames.AcceptEncoding));
#endif
        Assert.Equal("report.txt", result.Value!.FileName);
        Assert.Equal("text/plain", result.Value.MimeType);
        Assert.Equal(Encoding.UTF8.GetBytes("file"), result.Value.FileBytes);
    }

    [Fact]
    public async Task DownloadAsync_WithStringUrl_ConvertsUrlAndUsesDefaultGetMethod()
    {
        var service = new CaptureRequestHttpService
        {
            ResponseFactory = request =>
            {
                var response = new HttpResponse(request)
                {
                    StatusCode = HttpStatusCode.OK,
                    ResponseBytes = [],
                };
                response.VisitedUris.Add(request.GetUri());
                return response;
            },
        };

        var result = await service.DownloadAsync("https://example.com/files/report");

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.NotNull(service.Request);
        Assert.Equal(HttpMethod.Get, service.Request.Method);
        Assert.Equal(new Uri("https://example.com/files/report"), service.Request.GetUri());
    }

    [Fact]
    public async Task DownloadAsync_DisposesContentAfterSend()
    {
        var handler = new CaptureDownloadBodyHandler();
        using var service = HttpClientService.Create(
            () => new HttpClient(handler),
            disposeHttpClient: true,
            options: new()
            {
                RetryPolicyOptions = new()
                {
                    RetryCount = 0,
                },
            },
            useCookie: false);
        var content = new TrackingContent("payload");

        var result = await service.DownloadAsync(new DownloadOptions
        {
            Uri = new Uri("https://example.com/file.txt"),
            Method = HttpMethod.Post,
            Content = content,
        });

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Equal("payload", handler.RequestBody);
        Assert.True(content.IsDisposed);
    }

    private sealed class CaptureRequestHttpService : IHttpService
    {
        public HttpRequest? Request { get; private set; }
        public List<AddCookieCall> AddCookieCalls { get; } = [];
        public Dictionary<Uri, IReadOnlyCollection<Cookie>> CookiesByUri { get; } = [];
        public List<Cookie> AllCookies { get; } = [];
        public Uri? LastGetCookieUri { get; private set; }
        public string? LastGetCookieName { get; private set; }
        public Uri? LastGetCookiesUri { get; private set; }
        public Func<HttpRequest, HttpResponse>? ResponseFactory { get; init; }

        public Task<HttpResponse> SendAsync(HttpRequest request, CancellationToken token = default)
        {
            Request = request;
            if (ResponseFactory is not null)
                return Task.FromResult(ResponseFactory(request));

            return Task.FromResult(HttpResponse.FromError(request, new InvalidOperationException("Stop after capturing request.")));
        }

        public void AddCookie(Cookie cookie, Uri? uri = null, bool overrideDomain = false)
        {
            AddCookieCalls.Add(new(cookie, uri, overrideDomain));
        }

        public Cookie? GetCookie(Uri uri, string name)
        {
            LastGetCookieUri = uri;
            LastGetCookieName = name;
            return GetCookies(uri).FirstOrDefault(m => m.Name == name);
        }

        public IReadOnlyCollection<Cookie> GetCookies(Uri uri)
        {
            LastGetCookiesUri = uri;
            return CookiesByUri.TryGetValue(uri, out var cookies) ? cookies : [];
        }

        public IReadOnlyCollection<Cookie> GetAllCookies() => AllCookies;

        public IWebProxy? Proxy { get; set; }

        public ILogger Logger { get; set; } = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

        public void Dispose() { }

        public readonly record struct AddCookieCall(Cookie Cookie, Uri? Uri, bool OverrideDomain);
    }

    private sealed class CaptureDownloadBodyHandler : HttpMessageHandler
    {
        public string? RequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestBody = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                RequestMessage = request,
                Content = new ByteArrayContent(Encoding.UTF8.GetBytes("file")),
            };
        }
    }

    private sealed class TrackingContent(string value) : HttpContent
    {
        public bool IsDisposed { get; private set; }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            return stream.WriteAsync(bytes, 0, bytes.Length);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = Encoding.UTF8.GetByteCount(value);
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                IsDisposed = true;

            base.Dispose(disposing);
        }
    }
}
