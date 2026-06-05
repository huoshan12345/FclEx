namespace FclEx.Http.Services;

[CollectionDefinition(nameof(HttpClientServiceCacheCollection), DisableParallelization = true)]
public sealed class HttpClientServiceCacheCollection;

[Collection(nameof(HttpClientServiceCacheCollection))]
public partial class HttpClientServiceTests
{
    [Fact]
    public void MaxCacheCount_WhenCacheIsCreated_CannotBeChanged()
    {
        _ = HttpClientService.GetProvider(new HttpClientOptions());

        Assert.Throws<InvalidOperationException>(() => HttpClientService.MaxCacheCount = HttpClientService.MaxCacheCount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void MaxCacheCount_WhenValueIsNotPositive_Throws(int value)
    {
        Assert.ThrowsAny<ArgumentOutOfRangeException>(() => HttpClientService.MaxCacheCount = value);
    }

    [Fact]
    public void ClearCache_WhenCacheExists_DisposesCachedProvidersAndAllowsRecreation()
    {
        var options = new HttpClientOptions
        {
            HandlerOptions = new SocketsHttpHandlerOptions
            {
                Proxy = WebProxyHelper.Create("http://127.0.0.1:18080"),
            },
        };
        var provider = HttpClientService.GetProvider(options);
        _ = provider.GetRequiredService<IHttpClientFactory>();

        HttpClientService.ClearCache();

        Assert.Throws<ObjectDisposedException>(() => provider.GetRequiredService<IHttpClientFactory>());

        var newProvider = HttpClientService.GetProvider(options);
        Assert.NotSame(provider, newProvider);
        _ = newProvider.GetRequiredService<IHttpClientFactory>();
    }
}
