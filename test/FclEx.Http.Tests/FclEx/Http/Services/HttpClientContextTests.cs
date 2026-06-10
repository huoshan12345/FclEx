namespace FclEx.Http.Services;

public class HttpClientContextTests
{
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
