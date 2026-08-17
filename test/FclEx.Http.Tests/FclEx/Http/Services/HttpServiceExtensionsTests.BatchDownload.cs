namespace FclEx.Http.Services;

public class HttpServiceExtensionsBatchDownloadTests
{
    [Fact]
    public async Task BatchDownloadAsync_WithStringUris_ConvertsEachUri()
    {
        var service = new CaptureDownloadRequestService();

        var results = await service.BatchDownloadAsync(
            [
                "https://example.com/one.txt",
                "two.txt",
            ],
            new()
            {
                BaseAddress = new Uri("https://example.com/root/"),
                MaxDegreeOfParallelism = 1,
            });

        Assert.All(results, result => Assert.True(result.IsSuccess, result.Exception?.ToString()));
        Assert.Equal(
        [
            new Uri("https://example.com/one.txt"),
            new Uri("https://example.com/root/two.txt"),
        ], service.Requests.Select(m => m.GetUri()));
    }

    [Fact]
    public async Task BatchDownloadAsync_WhenOptionsAreNull_UsesDefaultOptions()
    {
        var service = new CaptureDownloadRequestService();

        var results = await service.BatchDownloadAsync(
        [
            new Uri("https://example.com/one.txt"),
        ]);

        var result = Assert.Single(results);
        Assert.True(result.IsSuccess, result.Exception?.ToString());
        var request = Assert.Single(service.Requests);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal(HttpContentType.Bytes, request.ResponseContentType);
        Assert.Null(request.Content);
    }

    [Fact]
    public async Task BatchDownloadAsync_WhenUrisAreEmpty_ReturnsEmptyArray()
    {
        var service = new CaptureDownloadRequestService();

        var results = await service.BatchDownloadAsync(Array.Empty<Uri>(), new() { MaxDegreeOfParallelism = 1 });

        Assert.Empty(results);
        Assert.Empty(service.Requests);
    }

    [Fact]
    public async Task BatchDownloadAsync_WhenBaseAddressIsProvided_ResolvesRelativeUris()
    {
        var service = new CaptureDownloadRequestService();

        var results = await service.BatchDownloadAsync(
            [
                new Uri("files/one.txt", UriKind.Relative),
                new Uri("/files/two.txt", UriKind.Relative),
                new Uri("https://other.example.com/three.txt"),
            ],
            new()
            {
                BaseAddress = new Uri("https://example.com/root/"),
                MaxDegreeOfParallelism = 1,
            });

        Assert.All(results, result => Assert.True(result.IsSuccess, result.Exception?.ToString()));
        Assert.Equal(
        [
            new Uri("https://example.com/root/files/one.txt"),
            new Uri("https://example.com/files/two.txt"),
            new Uri("https://other.example.com/three.txt"),
        ], service.Requests.Select(m => m.GetUri()));
    }

    [Fact]
    public async Task BatchDownloadAsync_MapsOptionsToEachDownloadRequest()
    {
        var service = new CaptureDownloadRequestService();
        using var cts = new CancellationTokenSource();
        var readHeadersTimeout = TimeSpan.FromSeconds(3);
        var readBufferTimeout = TimeSpan.FromSeconds(5);

        var results = await service.BatchDownloadAsync(
            [
                new Uri("https://example.com/one.txt"),
                new Uri("https://example.com/two.txt"),
            ],
            new()
            {
                Method = HttpMethod.Post,
                BufferSize = 1234,
                ReadHeadersTimeout = readHeadersTimeout,
                ReadBufferTimeout = readBufferTimeout,
                CancellationToken = cts.Token,
                MaxDegreeOfParallelism = 1,
            });

        Assert.All(results, result => Assert.True(result.IsSuccess, result.Exception?.ToString()));
        Assert.All(service.Requests, request =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal(1234, request.BufferSize);
            Assert.Equal(readHeadersTimeout, request.ReadHeadersTimeout);
            Assert.Equal(readBufferTimeout, request.ReadBufferTimeout);
            Assert.Equal(HttpContentType.Bytes, request.ResponseContentType);
        });
        Assert.All(service.Tokens, token => Assert.Equal(cts.Token, token));
    }

    [Fact]
    public async Task BatchDownloadAsync_WhenDownloadFails_ReturnsErrorResult()
    {
        var exception = new InvalidOperationException("download failed");
        var service = new CaptureDownloadRequestService
        {
            Exception = exception,
        };

        var results = await service.BatchDownloadAsync(
            [
                new Uri("https://example.com/file.txt"),
            ],
            new()
            {
                MaxDegreeOfParallelism = 1,
            });

        var result = Assert.Single(results);
        Assert.True(result.IsError);
        Assert.Contains(result.Exception.EnumerateInner(), m => ReferenceEquals(exception, m));
    }

    [Fact]
    public async Task BatchDownloadAsync_WhenContentIsProvided_BuffersOnceAndClonesForEachRequest()
    {
        var handler = new CaptureBodyHandler();
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
        var content = new SingleReadContent("payload");

        var results = await service.BatchDownloadAsync(
            [
                new Uri("https://example.com/one"),
                new Uri("https://example.com/two"),
            ],
            new()
            {
                Method = HttpMethod.Post,
                Content = content,
                MaxDegreeOfParallelism = 1,
            });

        Assert.All(results, result => Assert.True(result.IsSuccess, result.Exception?.ToString()));
        Assert.Equal(1, content.SerializeCount);
        Assert.True(content.IsDisposed);
        Assert.Equal(["payload", "payload"], handler.RequestBodies);
    }

    private sealed class CaptureBodyHandler : HttpMessageHandler
    {
        public List<string> RequestBodies { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestBodies.Add(request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken));

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                RequestMessage = request,
                Content = new ByteArrayContent(Encoding.UTF8.GetBytes("file")),
            };
        }
    }

    private sealed class CaptureDownloadRequestService : IHttpService
    {
        public List<HttpRequest> Requests { get; } = [];

        public List<CancellationToken> Tokens { get; } = [];

        public Exception? Exception { get; init; }

        public Task<HttpResponse> SendAsync(HttpRequest request, CancellationToken token = default)
        {
            Requests.Add(request);
            Tokens.Add(token);

            if (Exception is not null)
                return Task.FromResult(HttpResponse.FromError(request, Exception));

            var response = new HttpResponse(request)
            {
                StatusCode = HttpStatusCode.OK,
                ResponseBytes = Encoding.UTF8.GetBytes("file"),
            };
            response.Headers.Add(HttpHeaderNames.ContentType, MediaTypes.Text);
            response.VisitedUris.Add(request.GetUri());
            return Task.FromResult(response);
        }

        public void AddCookie(Cookie cookie, Uri? uri = null, bool overrideDomain = false) { }

        public Cookie? GetCookie(Uri uri, string name) => null;

        public IReadOnlyCollection<Cookie> GetCookies(Uri uri) => [];

        public IReadOnlyCollection<Cookie> GetAllCookies() => [];

        public IWebProxy? Proxy { get; set; }

        public ILogger Logger { get; set; } = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

        public void Dispose() { }
    }

    private sealed class SingleReadContent(string value) : HttpContent
    {
        public int SerializeCount { get; private set; }

        public bool IsDisposed { get; private set; }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            SerializeCount++;
            if (SerializeCount > 1)
            {
                throw new InvalidOperationException("The source content can only be serialized once.");
            }

            var bytes = Encoding.UTF8.GetBytes(value);
            await stream.WriteAsync(bytes, 0, bytes.Length);
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
