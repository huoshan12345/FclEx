namespace FclEx.Logging.Extensions;

public class LoggingBuilderExtensionsTests
{
    [Fact]
    public void AddCollecting_RespectsFactoryFiltersAndClearProviders()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddCollecting().ClearProviders().AddCollecting()
            .SetMinimumLevel(LogLevel.Warning));
        using var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<LoggingBuilderExtensionsTests>>();
        var provider = serviceProvider.GetRequiredService<CollectingLoggerProvider>();
        logger.LogInformation("filtered");
        logger.LogWarning("collected");
        Assert.Equal("collected", Assert.Single(provider.Entries).Message);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AddCollecting_RepeatedCalls_RegisterOneProviderAndCollectOnce(bool repeatAddLogging)
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            Assert.Same(builder, builder.AddCollecting());
            Assert.Same(builder, builder.AddCollecting());
        });

        if (repeatAddLogging)
            services.AddLogging(builder => builder.AddCollecting());

        Assert.Single(services, descriptor => descriptor.ServiceType == typeof(ILoggerProvider));
        using var serviceProvider = services.BuildServiceProvider();
        var provider = serviceProvider.GetRequiredService<CollectingLoggerProvider>();
        Assert.Same(provider, Assert.Single(serviceProvider.GetServices<ILoggerProvider>()));

        var logger = serviceProvider.GetRequiredService<ILogger<LoggingBuilderExtensionsTests>>();
        logger.LogInformation("message");

        Assert.Equal("message", Assert.Single(provider.Entries).Message);
    }

    [Fact]
    public void AddCollecting_PreservesExistingSingletonAndOtherProviders()
    {
        using var collectingProvider = new CollectingLoggerProvider();
        using var otherProvider = new OtherLoggerProvider();
        var services = new ServiceCollection();
        services.AddSingleton(collectingProvider);
        services.AddSingleton<ILoggerProvider>(otherProvider);
        services.AddLogging(builder => builder.AddCollecting());

        using var serviceProvider = services.BuildServiceProvider();
        var providers = serviceProvider.GetServices<ILoggerProvider>().ToArray();
        Assert.Equal(2, providers.Length);
        Assert.Contains(otherProvider, providers);
        Assert.Contains(collectingProvider, providers);
        Assert.Same(collectingProvider, serviceProvider.GetRequiredService<CollectingLoggerProvider>());

        var logger = serviceProvider.GetRequiredService<ILogger<LoggingBuilderExtensionsTests>>();
        logger.LogInformation("message");

        Assert.Equal("message", Assert.Single(collectingProvider.Entries).Message);
        Assert.Equal("message", Assert.Single(otherProvider.Logger.Entries).Message);
    }

    private sealed class OtherLoggerProvider : ILoggerProvider
    {
        public CollectingLogger Logger { get; } = new();

        public ILogger CreateLogger(string categoryName) => Logger;

        public void Dispose() { }
    }
}
