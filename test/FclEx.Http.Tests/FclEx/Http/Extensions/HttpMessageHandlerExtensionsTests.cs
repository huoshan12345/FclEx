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
    public void EnumerateInner_WhenDelegatingHandlerHasNoInnerHandler_ReturnsOnlyDelegatingHandler()
    {
        var handler = new PassThroughHandler();

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

    [Fact]
    public void CreateSocketsHttpHandler_ByDefault_UsesDefaultServerCertificateValidation()
    {
        using var handler = HttpMessageHandler.CreateSocketsHttpHandler();

        Assert.Null(handler.SslOptions.RemoteCertificateValidationCallback);
    }

    [Fact]
    public void CreateSocketsHttpHandler_WhenCertificateValidationIsDisabled_BypassesServerCertificateValidation()
    {
        using var handler = HttpMessageHandler.CreateSocketsHttpHandler(new()
        {
            DisableServerCertificateValidation = true,
        });

        var callback = handler.SslOptions.RemoteCertificateValidationCallback;

        Assert.NotNull(callback);
        Assert.True(callback(null!, null, null, SslPolicyErrors.RemoteCertificateNameMismatch));
    }

    [Fact]
    public void CreateSocketsHttpHandler_CopiesOptionsAndDisablesCookieContainer()
    {
        var proxy = new WebProxy("http://127.0.0.1:8888");
        var options = new SocketsHttpHandlerOptions
        {
            ConnectTimeout = TimeSpan.FromSeconds(3),
            PooledConnectionLifetime = TimeSpan.FromMinutes(2),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            Proxy = proxy,
#if NET6_0_OR_GREATER
            EnableMultipleHttp2Connections = true,
#endif
        };

        using var handler = HttpMessageHandler.CreateSocketsHttpHandler(options);

        Assert.Equal(options.ConnectTimeout, handler.ConnectTimeout);
        Assert.Equal(options.PooledConnectionLifetime, handler.PooledConnectionLifetime);
        Assert.Equal(options.PooledConnectionIdleTimeout, handler.PooledConnectionIdleTimeout);
        Assert.Equal(int.MaxValue, handler.MaxConnectionsPerServer);
        Assert.False(handler.UseCookies);
        Assert.False(handler.AllowAutoRedirect);
        Assert.Equal(options.AutomaticDecompression, handler.AutomaticDecompression);
        Assert.True(handler.UseProxy);
        Assert.Same(proxy, handler.Proxy);
#if NET6_0_OR_GREATER
        Assert.True(handler.EnableMultipleHttp2Connections);
#endif
    }

    private sealed class PassThroughHandler : DelegatingHandler;

    private sealed class TerminalHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
