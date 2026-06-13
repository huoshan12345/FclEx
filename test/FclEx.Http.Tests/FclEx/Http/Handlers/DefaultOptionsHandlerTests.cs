#if NET5_0_OR_GREATER
namespace FclEx.Http.Handlers;

public class DefaultOptionsHandlerTests
{
    [Fact]
    public void SetOption_StoresOptionAndReturnsSameHandler()
    {
        var key = new HttpRequestOptionsKey<int>("retry-count");
        using var handler = new DefaultOptionsHandler();

        var result = handler.SetOption(key, 3);

        Assert.Same(handler, result);
        Assert.True(handler.Options.TryGetValue(key, out var value));
        Assert.Equal(3, value);
    }

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

    [Fact]
    public async Task SendAsync_AppliesMultipleConfiguredOptions_ToRequest()
    {
        var stringKey = new HttpRequestOptionsKey<string>("name");
        var intKey = new HttpRequestOptionsKey<int>("count");
        using var handler = new DefaultOptionsHandler()
        {
            InnerHandler = new CaptureMultipleOptionsHandler(stringKey, intKey),
        };
        handler
            .SetOption(stringKey, "fclex")
            .SetOption(intKey, 42);
        using var invoker = new HttpMessageInvoker(handler);

        using var response = await invoker.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, "https://example.com"),
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SendAsync_OverwritesExistingRequestOption()
    {
        var key = new HttpRequestOptionsKey<string>("name");
        using var handler = new DefaultOptionsHandler()
        {
            InnerHandler = new CaptureOptionsHandler(key),
        };
        handler.SetOption(key, "expected");
        using var invoker = new HttpMessageInvoker(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
        request.Options.Set(key, "original");

        using var response = await invoker.SendAsync(request, CancellationToken.None);

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

    private sealed class CaptureMultipleOptionsHandler(
        HttpRequestOptionsKey<string> stringKey,
        HttpRequestOptionsKey<int> intKey) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Assert.True(request.Options.TryGetValue(stringKey, out var stringValue));
            Assert.True(request.Options.TryGetValue(intKey, out var intValue));
            Assert.Equal("fclex", stringValue);
            Assert.Equal(42, intValue);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
#endif
