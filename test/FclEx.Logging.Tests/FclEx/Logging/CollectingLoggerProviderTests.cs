namespace FclEx.Logging;

public class CollectingLoggerProviderTests
{
    [Fact]
    public void Factory_ScopesAreSharedAcrossCategoriesAndRestoredOnDispose()
    {
        using var provider = new CollectingLoggerProvider();
        using var factory = LoggerFactory.Create(builder => builder.AddProvider(provider));
        var first = factory.CreateLogger("first");
        var second = factory.CreateLogger("second");

        using (first.BeginScope("outer"))
        {
            using (second.BeginScope("inner"))
            {
                first.LogInformation("nested");
            }

            second.LogInformation("outer only");
        }

        first.LogInformation("no scopes");

        Assert.Equal("first", provider.Entries[0].Category);
        Assert.Equal("second", provider.Entries[1].Category);
        Assert.Equal(new object?[] { "outer", "inner" }, provider.Entries[0].Scopes);
        Assert.Equal(new object?[] { "outer" }, provider.Entries[1].Scopes);
        Assert.Empty(provider.Entries[2].Scopes);
    }

    [Fact]
    public void SetScopeProvider_ExistingLoggersUseTheNewProvider()
    {
        using var provider = new CollectingLoggerProvider();
        var logger = provider.CreateLogger("category");
        var aggregateLogger = provider.Logger;
        var scopes = new LoggerExternalScopeProvider();
        provider.SetScopeProvider(scopes);

        using (scopes.Push("external"))
        {
            logger.LogInformation("category message");
            aggregateLogger.LogInformation("aggregate message");
        }

        using (logger.BeginScope("logger scope"))
        {
            aggregateLogger.LogInformation("shared message");
        }

        Assert.Equal(new object?[] { "external" }, provider.Entries[0].Scopes);
        Assert.Equal(new object?[] { "external" }, provider.Entries[1].Scopes);
        Assert.Equal(new object?[] { "logger scope" }, provider.Entries[2].Scopes);
    }

    [Fact]
    public void AddCollecting_FactoryCapturesPropertyScopesInResolvedProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddCollecting());
        using var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<CollectingLoggerProviderTests>>();
        var provider = serviceProvider.GetRequiredService<CollectingLoggerProvider>();

        using (logger.PushProperty("request", 42))
        {
            logger.LogInformation("message");
        }

        var entry = Assert.Single(provider.Entries);
        var scope = Assert.Single(entry.Scopes);
        var properties = Assert.IsAssignableFrom<IEnumerable<KeyValuePair<string, object?>>>(scope);
        Assert.Equal(new KeyValuePair<string, object?>("request", 42), Assert.Single(properties));
    }
}
