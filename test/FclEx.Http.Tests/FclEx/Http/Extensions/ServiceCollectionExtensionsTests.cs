namespace FclEx.Http.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddHttpClientWithPolly_ConfiguresNamedHttpClientFromOptions()
    {
        var services = new ServiceCollection();
        var options = new HttpClientOptions
        {
            BaseAddress = new Uri("https://example.test/api/"),
            TotalTimeout = TimeSpan.FromSeconds(17),
            RetryPolicyOptions = new()
            {
                AutoUpdateTotalTimeout = false,
            },
#if NET6_0_OR_GREATER
            HttpVersion = HttpVersion.Version20,
            HttpVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher,
#endif
        };
        services.AddHttpClientWithPolly("configured", options);
        using var provider = services.BuildServiceProvider();

        var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient("configured");

        Assert.Equal(options.BaseAddress, client.BaseAddress);
        Assert.Equal(options.TotalTimeout, client.Timeout);
#if NET6_0_OR_GREATER
        Assert.Equal(options.HttpVersion, client.DefaultRequestVersion);
        Assert.Equal(options.HttpVersionPolicy, client.DefaultVersionPolicy);
#endif
    }

    [Fact]
    public void AddHttpClientWithPolly_WhenOptionsFactoryIsUsed_ResolvesOptionsFromServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new ClientOptionsProvider
        {
            Options = new()
            {
                BaseAddress = new Uri("https://factory.example.test/"),
                TotalTimeout = TimeSpan.FromSeconds(19),
                RetryPolicyOptions = new()
                {
                    AutoUpdateTotalTimeout = false,
                },
            },
        });
        services.AddHttpClientWithPolly(
            "configured-by-factory",
            m => m.GetRequiredService<ClientOptionsProvider>().Options);
        using var provider = services.BuildServiceProvider();

        var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient("configured-by-factory");

        Assert.Equal(new Uri("https://factory.example.test/"), client.BaseAddress);
        Assert.Equal(TimeSpan.FromSeconds(19), client.Timeout);
    }

    private sealed class ClientOptionsProvider
    {
        public HttpClientOptions Options { get; init; } = new();
    }
}
