namespace FclEx.Http.Services;

public class HttpServiceExtensionsBatchDownloadTests
{
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
                ExecuteInParallel = false,
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
