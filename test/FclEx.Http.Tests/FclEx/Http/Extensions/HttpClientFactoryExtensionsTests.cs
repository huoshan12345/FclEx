namespace FclEx.Http.Extensions;

public class HttpClientFactoryExtensionsTests
{
    [Fact]
    public async Task CreateHttpService_WhenNameIsProvided_UsesNamedClientFromFactory()
    {
        var factory = new FakeHttpClientFactory();
        using var client = new TrackingHttpClient(new CaptureRequestHandler());
        factory.Clients["api"] = client;
        using var service = factory.CreateHttpService(
            "api",
            options: new HttpClientOptions
            {
                RetryPolicyOptions = new()
                {
                    RetryCount = 0,
                },
            },
            useCookie: false);
        var request = new HttpRequest(new Uri("https://example.test/api"), HttpMethod.Get);

        var response = await service.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(["api"], factory.Names);
        Assert.False(client.Disposed);
    }

    [Fact]
    public async Task CreateHttpService_WhenNameIsNull_UsesHttpClientServiceName()
    {
        var factory = new FakeHttpClientFactory();
        using var client = new TrackingHttpClient(new CaptureRequestHandler());
        factory.Clients[nameof(HttpClientService)] = client;
        using var service = factory.CreateHttpService(
            options: new HttpClientOptions
            {
                RetryPolicyOptions = new()
                {
                    RetryCount = 0,
                },
            },
            useCookie: false);
        var request = new HttpRequest(new Uri("https://example.test/default"), HttpMethod.Get);

        var response = await service.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal([nameof(HttpClientService)], factory.Names);
        Assert.False(client.Disposed);
    }

    private sealed class FakeHttpClientFactory : IHttpClientFactory
    {
        public Dictionary<string, HttpClient> Clients { get; } = [];

        public List<string> Names { get; } = [];

        public HttpClient CreateClient(string name)
        {
            Names.Add(name);
            return Clients[name];
        }
    }

    private sealed class TrackingHttpClient(HttpMessageHandler handler) : HttpClient(handler, disposeHandler: true)
    {
        public bool Disposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            Disposed = true;
            base.Dispose(disposing);
        }
    }

    private sealed class CaptureRequestHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                RequestMessage = request,
                Content = new StringContent("ok"),
            });
        }
    }
}
