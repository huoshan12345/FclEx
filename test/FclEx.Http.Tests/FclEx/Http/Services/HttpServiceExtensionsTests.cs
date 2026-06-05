namespace FclEx.Http.Services;

public class HttpServiceExtensionsTests
{
    [RetryTheory]
    [InlineData("https://www.google.com/", "www_google_com.html")]
    [InlineData("https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-9.0/covariant-returns", "covariant-returns.html")]
    [InlineData("https://devblogs.microsoft.com/dotnet/csharp-exploring-extension-members/#comments", "csharp-exploring-extension-members.html")]
    public async Task DownloadAsync_Test(string uri, string fileName)
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
    public async Task DownloadAsync_MapsReadHeadersTimeoutToRequest()
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

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DownloadAsync_RespectsDisposeContentOption(bool disposeContent)
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
            DisposeContent = disposeContent,
        });

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Equal("payload", handler.RequestBody);
        Assert.Equal(disposeContent, content.IsDisposed);

        if (content.IsDisposed == false)
            content.Dispose();
    }

    private sealed class CaptureRequestHttpService : IHttpService
    {
        public HttpRequest? Request { get; private set; }

        public Task<HttpResponse> SendAsync(HttpRequest request, CancellationToken token = default)
        {
            Request = request;
            return Task.FromResult(HttpResponse.FromError(request, new InvalidOperationException("Stop after capturing request.")));
        }

        public void AddCookie(Cookie cookie, Uri? uri = null, bool overrideDomain = false) { }

        public Cookie? GetCookie(Uri uri, string name) => null;

        public IReadOnlyCollection<Cookie> GetCookies(Uri uri) => [];

        public IReadOnlyCollection<Cookie> GetAllCookies() => [];

        public IWebProxy? Proxy { get; set; }

        public ILogger Logger { get; set; } = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

        public void Dispose() { }
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
