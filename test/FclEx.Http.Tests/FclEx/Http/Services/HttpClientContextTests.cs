namespace FclEx.Http.Services;

public class HttpClientContextTests
{
    [Fact]
    public void Constructor_StoresClientPolicyAndDisposeFlag()
    {
        using var client = new HttpClient(new TrackingHandler());
        var policy = Polly.Policy.NoOpAsync<HttpResponseMessage>();

        var context = new HttpClientContext(client, policy, DisposeHttpClient: false);

        Assert.Same(client, context.Client);
        Assert.Same(policy, context.Policy);
        Assert.False(context.DisposeHttpClient);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Dispose_OnlyDisposesClientWhenDisposeHttpClientIsTrue(bool disposeHttpClient)
    {
        var handler = new TrackingHandler();
        using var client = new HttpClient(handler);
        var context = new HttpClientContext(
            client,
            Polly.Policy.NoOpAsync<HttpResponseMessage>(),
            disposeHttpClient);

        context.Dispose();

        Assert.Equal(disposeHttpClient, handler.IsDisposed);
    }

    private sealed class TrackingHandler : HttpMessageHandler
    {
        public bool IsDisposed { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }
    }
}
