namespace FclEx.Logging.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void RemoveLogging_NullCollection_ThrowsArgumentNullException()
    {
        Assert.Equal("services", Assert.Throws<ArgumentNullException>(() =>
            ServiceCollectionExtensions.RemoveLogging(null!)).ParamName);
    }

    [Fact]
    public void RemoveLogging_RemovesOpenAndClosedLoggerRegistrationsAndPreservesOtherServices()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddCollecting());
        services.AddSingleton<ILogger>(new CollectingLogger());
        services.AddSingleton<ILogger<ServiceCollectionExtensionsTests>, Logger<ServiceCollectionExtensionsTests>>();
        services.AddSingleton("unrelated");

        Assert.Same(services, services.RemoveLogging());
        services.RemoveLogging();

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });
        Assert.Null(provider.GetService<ILoggerFactory>());
        Assert.Empty(provider.GetServices<ILoggerProvider>());
        Assert.Null(provider.GetService<ILogger>());
        Assert.Null(provider.GetService<ILogger<ServiceCollectionExtensionsTests>>());
        Assert.Equal("unrelated", provider.GetRequiredService<string>());
    }

    [Fact]
    public void RemoveLogging_AddLoggingAgain_RestoresTypedLoggerResolution()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.RemoveLogging();
        services.AddLogging(builder => builder.AddCollecting());
        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<ILogger<ServiceCollectionExtensionsTests>>().LogInformation("message");

        Assert.Equal("message", Assert.Single(provider.GetRequiredService<CollectingLoggerProvider>().Entries).Message);
    }
}
