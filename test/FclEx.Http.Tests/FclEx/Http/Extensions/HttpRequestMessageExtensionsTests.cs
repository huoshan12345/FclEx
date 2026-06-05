namespace FclEx.Http.Extensions;

public class HttpRequestMessageExtensionsTests
{
    [Fact]
    public async Task CloneAsync_CopiesRequestMetadataHeadersOptionsAndBufferedContent()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://example.com/api")
        {
            Version = new Version(2, 0),
#if NET5_0_OR_GREATER
            VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher,
#endif
            Content = new StringContent("payload", Encoding.UTF8, "text/plain"),
        };
        request.Headers.TryAddWithoutValidation("X-Test", ["one", "two"]);
        request.Content.Headers.TryAddWithoutValidation("X-Content", "content-header");
#if NET5_0_OR_GREATER
        var optionKey = new HttpRequestOptionsKey<string>("option-key");
        request.Options.Set(optionKey, "option-value");
#else
        request.Properties["option-key"] = "option-value";
#endif

        using var clone = await request.CloneAsync();
        request.Content.Dispose();

        Assert.NotSame(request, clone);
        Assert.Equal(request.Method, clone.Method);
        Assert.Equal(request.RequestUri, clone.RequestUri);
        Assert.Equal(request.Version, clone.Version);
#if NET5_0_OR_GREATER
        Assert.Equal(request.VersionPolicy, clone.VersionPolicy);
        Assert.True(clone.Options.TryGetValue(optionKey, out var optionValue));
        Assert.Equal("option-value", optionValue);
#else
        Assert.Equal("option-value", clone.Properties["option-key"]);
#endif
        Assert.Equal(["one", "two"], clone.Headers.GetValues("X-Test"));
        Assert.Equal("content-header", Assert.Single(clone.Content!.Headers.GetValues("X-Content")));
        Assert.Equal("payload", await clone.Content.ReadAsStringAsync());
    }
}
