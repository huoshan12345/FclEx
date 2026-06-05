#if NET5_0_OR_GREATER
namespace FclEx.Http.Handlers;

public class DefaultOptionsHandlerTests
{
    [Fact]
    public async Task SendAsync_AppliesConfiguredOptions_ToRequest()
    {
        var key = new HttpRequestOptionsKey<string>("fclex-test-option");
        using var handler = new DefaultOptionsHandler()
        {
            InnerHandler = new CaptureOptionsHandler(key),
        };
        handler.SetOption(key, "expected");
        using var invoker = new HttpMessageInvoker(handler);

        using var response = await invoker.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, "https://example.com"),
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private sealed class CaptureOptionsHandler(HttpRequestOptionsKey<string> key) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Assert.True(request.Options.TryGetValue(key, out var value));
            Assert.Equal("expected", value);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
#endif
