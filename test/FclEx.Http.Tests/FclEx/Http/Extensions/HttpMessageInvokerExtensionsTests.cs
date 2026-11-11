namespace FclEx.Http.Extensions;

public class HttpMessageInvokerExtensionsTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SetHandler_UsesReplacementAndPreservesDisposalOwnership(bool disposeHandler)
    {
        using var original = new TrackingHandler(HttpStatusCode.Accepted);
        using var replacement = new TrackingHandler(HttpStatusCode.Created);
        var invoker = new HttpMessageInvoker(original, disposeHandler);
        using (invoker)
        {
            Assert.Same(invoker, invoker.SetHandler(replacement));
            Assert.Same(replacement, invoker.GetHandler());
            Assert.False(original.Disposed);
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            using var response = await invoker.SendAsync(request, CancellationToken.None);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }
        Assert.False(original.Disposed);
        Assert.Equal(disposeHandler, replacement.Disposed);
    }

    [Fact]
    public async Task SetHandler_HttpClient_PreservesFluentTypeAndUsesReplacement()
    {
        using var original = new TrackingHandler(HttpStatusCode.Accepted);
        using var replacement = new TrackingHandler(HttpStatusCode.Created);
        using var client = new HttpClient(original, disposeHandler: false);
        HttpClient result = client.SetHandler(replacement);
        Assert.Same(client, result);
        using var response = await client.GetAsync("https://example.com", CancellationToken.None);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public void GetHandler_DoesNotSearchDelegatingHandlerChain()
    {
        using var outer = new DelegatingRoot { InnerHandler = new TerminalHandler() };
        using var invoker = new HttpMessageInvoker(outer, disposeHandler: false);
        Assert.Same(outer, invoker.GetHandler());
        Assert.Throws<InvalidCastException>(() => invoker.GetHandler<TerminalHandler>());
    }

    private sealed class DelegatingRoot : DelegatingHandler;

    private sealed class TrackingHandler(HttpStatusCode statusCode) : HttpMessageHandler
    {
        public bool Disposed { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(statusCode));

        protected override void Dispose(bool disposing)
        {
            Disposed = true;
            base.Dispose(disposing);
        }
    }

    [Fact]
    public void GetHandler_ReturnsHandlerStoredByInvoker()
    {
        var handler = new TerminalHandler();
        using var invoker = new HttpMessageInvoker(handler, disposeHandler: true);

        var actual = invoker.GetHandler();

        Assert.Same(handler, actual);
    }

    [Fact]
    public void GetHandler_WithTypedHandler_ReturnsHandlerAsRequestedType()
    {
        var handler = new TerminalHandler();
        using var invoker = new HttpMessageInvoker(handler, disposeHandler: true);

        var actual = invoker.GetHandler<TerminalHandler>();

        Assert.Same(handler, actual);
    }

    private sealed class TerminalHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
