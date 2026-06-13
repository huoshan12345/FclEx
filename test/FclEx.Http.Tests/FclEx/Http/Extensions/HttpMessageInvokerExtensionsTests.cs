namespace FclEx.Http.Extensions;

public class HttpMessageInvokerExtensionsTests
{
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
