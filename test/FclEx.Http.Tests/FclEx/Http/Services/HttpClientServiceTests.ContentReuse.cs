namespace FclEx.Http.Services;

public partial class HttpClientServiceTests
{
    public sealed class TestHttpClientService : HttpClientService
    {
        public static HttpRequestMessage BuildHttpRequest(HttpRequest request, BufferedContent content)
        {
            return BuildHttpRequest(request, content, null, new CookieContainer(), CancellationToken.None);
        }
    }

    [Fact]
    public async Task BuildHttpRequest_WhenBufferedContentIsUsedAfterPreviousMessageIsDisposed_CanBuildContentAgain()
    {
        var request = HttpRequest.Post("https://example.com/api")
            .StringContent("payload");
        using var content = await BufferedContent.CreateAsync(request.Content!);

        using (var firstMessage = TestHttpClientService.BuildHttpRequest(request, content))
        {
            var firstContent = await firstMessage.Content!.ReadAsStringAsync();
            Assert.Equal("payload", firstContent);
        }

        using var secondMessage = TestHttpClientService.BuildHttpRequest(request, content);
        var secondContent = await secondMessage.Content!.ReadAsStringAsync();

        Assert.Equal("payload", secondContent);
    }

    [Theory]
    [InlineData(CompressionMethod.None)]
    [InlineData(CompressionMethod.GZip)]
    [InlineData(CompressionMethod.Deflate)]
    public async Task SendAsync_WhenOuterRetryRebuildsRequest_CanResendContent(CompressionMethod compressionMethod)
    {
        var handler = new CancelOnceThenOkHandler();
        using var service = HttpClientService.Create(
            () => new HttpClient(handler),
            disposeHttpClient: true,
            options: new()
            {
                RetryCount = 1,
                SleepDurationProvider = _ => TimeSpan.Zero,
            },
            useCookie: false);

        var response = await HttpRequest.Post("https://example.com/api")
            .Compression(compressionMethod)
            .StringContent("payload")
            .SendAsync(service);

        Assert.False(response.IsError, response.Exception?.ToString());
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(new[] { "payload", "payload" }, handler.RequestContents);
    }

    [Fact]
    public async Task SendAsync_WhenOuterRetryRebuildsRequest_ReadsOriginalContentOnlyOnce()
    {
        var content = new SingleReadContent("payload");
        var handler = new CancelOnceThenOkHandler();
        using var service = HttpClientService.Create(
            () => new HttpClient(handler),
            disposeHttpClient: true,
            options: new()
            {
                RetryCount = 1,
                SleepDurationProvider = _ => TimeSpan.Zero,
            },
            useCookie: false);

        var response = await HttpRequest.Post("https://example.com/api")
            .Content(content)
            .SendAsync(service);

        Assert.False(response.IsError, response.Exception?.ToString());
        Assert.Equal(1, content.SerializeCount);
        Assert.Equal(new[] { "payload", "payload" }, handler.RequestContents);
    }

    [Fact]
    public async Task SendAsync_WhenTaskCanceledExceptionHasCustomMessage_DoesNotRetry()
    {
        var handler = new CustomCancellationHandler();
        using var service = HttpClientService.Create(
            () => new HttpClient(handler),
            disposeHttpClient: true,
            options: new()
            {
                RetryCount = 1,
                SleepDurationProvider = _ => TimeSpan.Zero,
            },
            useCookie: false);

        var response = await HttpRequest.Get("https://example.com/api")
            .SendAsync(service);

        Assert.True(response.IsError);
        Assert.IsType<TaskCanceledException>(response.Exception);
        Assert.Equal(1, handler.SendCount);
    }

    private sealed class CustomCancellationHandler : HttpMessageHandler
    {
        public int SendCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            SendCount++;
            throw new TaskCanceledException("The caller canceled this request explicitly.");
        }
    }

    private sealed class CancelOnceThenOkHandler : HttpMessageHandler
    {
        public List<string> RequestContents { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestContents.Add(await ReadRequestContentAsync(request, cancellationToken));

            if (RequestContents.Count == 1)
                throw new TaskCanceledException(Task.CompletedTask);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                RequestMessage = request,
                Content = new StringContent("ok"),
            };
        }
    }

    private sealed class SingleReadContent(string value) : HttpContent
    {
        public int SerializeCount { get; private set; }

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
    }

    private static async Task<string> ReadRequestContentAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var bytes = await request.Content!.ReadAsByteArrayAsync(cancellationToken);
        using var stream = new MemoryStream(bytes);
        var contentStream = request.Content.Headers.ContentEncoding.Contains("gzip", StringComparer.OrdinalIgnoreCase)
            ? new GZipStream(stream, CompressionMode.Decompress)
            : request.Content.Headers.ContentEncoding.Contains("deflate", StringComparer.OrdinalIgnoreCase)
                ? CreateDeflateStream(stream)
            : stream.CastTo<Stream>();
        using (contentStream)
        using (var reader = new StreamReader(contentStream, Encoding.UTF8))
        {
            return await reader.ReadToEndAsync();
        }
    }

    private static Stream CreateDeflateStream(Stream stream)
    {
#if NET5_0_OR_GREATER
        return new ZLibStream(stream, CompressionMode.Decompress);
#else
        return new DeflateStream(stream, CompressionMode.Decompress);
#endif
    }
}
