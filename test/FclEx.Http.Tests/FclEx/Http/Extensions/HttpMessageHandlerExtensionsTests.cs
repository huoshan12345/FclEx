namespace FclEx.Http.Extensions;

public class HttpMessageHandlerExtensionsTests
{
    [Fact]
    public void EnumerateInner_WhenHandlerIsNotDelegatingHandler_ReturnsOnlyHandler()
    {
        var handler = new TerminalHandler();

        var handlers = handler.EnumerateInner().ToArray();

        Assert.Single(handlers);
        Assert.Same(handler, handlers[0]);
    }

    [Fact]
    public void EnumerateInner_WhenHandlerChainHasPrimaryHandler_ReturnsChainAndStopsAtPrimaryHandler()
    {
        var primary = new TerminalHandler();
        var inner = new PassThroughHandler
        {
            InnerHandler = primary,
        };
        var outer = new PassThroughHandler
        {
            InnerHandler = inner,
        };

        var handlers = outer.EnumerateInner().ToArray();

        Assert.Equal(3, handlers.Length);
        Assert.Same(outer, handlers[0]);
        Assert.Same(inner, handlers[1]);
        Assert.Same(primary, handlers[2]);
    }

    private sealed class PassThroughHandler : DelegatingHandler
    {
    }

    private sealed class TerminalHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
