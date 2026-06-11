namespace FclEx.Http.Extensions;

public class HttpClientExtensionsTests
{
    [Fact]
    public void GetHandler_ReturnsRootHandlerStoredByHttpClient()
    {
        var handler = new TerminalHandler();
        using var client = new HttpClient(handler, disposeHandler: true);

        var actual = client.GetHandler();

        Assert.Same(handler, actual);
    }

    [Fact]
    public void GetPrimaryHandler_WhenClientUsesDelegatingHandlers_ReturnsLastHandlerInChain()
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
        using var client = new HttpClient(outer, disposeHandler: true);

        var actual = client.GetPrimaryHandler();

        Assert.Same(primary, actual);
    }

    [Fact]
    public void IgnoreRemoteCertificateValidation_WhenPrimaryHandlerIsHttpClientHandler_ConfiguresBypassCallback()
    {
        using var handler = new HttpClientHandler();
        using var client = new HttpClient(handler, disposeHandler: false);

        client.IgnoreRemoteCertificateValidation();

        Assert.Equal(ClientCertificateOption.Manual, handler.ClientCertificateOptions);
        Assert.NotNull(handler.ServerCertificateCustomValidationCallback);
        Assert.True(handler.ServerCertificateCustomValidationCallback(
            null!,
            null,
            null,
            SslPolicyErrors.RemoteCertificateNameMismatch));
    }

#if NET5_0_OR_GREATER
    [Fact]
    public void IgnoreRemoteCertificateValidation_WhenPrimaryHandlerIsSocketsHttpHandler_ConfiguresBypassCallback()
    {
        using var handler = new SocketsHttpHandler();
        using var client = new HttpClient(handler, disposeHandler: false);

        client.IgnoreRemoteCertificateValidation();

        Assert.NotNull(handler.SslOptions.RemoteCertificateValidationCallback);
        Assert.True(handler.SslOptions.RemoteCertificateValidationCallback(
            null!,
            null,
            null,
            SslPolicyErrors.RemoteCertificateNameMismatch));
    }
#endif

    private sealed class PassThroughHandler : DelegatingHandler;

    private sealed class TerminalHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
