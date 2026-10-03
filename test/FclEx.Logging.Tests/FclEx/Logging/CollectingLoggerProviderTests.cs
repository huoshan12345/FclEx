namespace FclEx.Logging;

public class CollectingLoggerProviderTests
{
    [Fact]
    public void CreateLogger_CollectorsShareEntriesAndClearing()
    {
        using var provider = new CollectingLoggerProvider();
        var first = Assert.IsType<CollectingLogger>(provider.CreateLogger("first"));
        var second = provider.CreateLogger("second");
        first.LogInformation("first");
        second.LogInformation("second");
        Assert.Equal(2, first.Entries.Count);
        first.Clear();
        Assert.Empty(provider.Entries);
        second.LogInformation("after clear");
        Assert.Equal("after clear", Assert.Single(first.Entries).Message);
    }

    [Fact]
    public void SetScopeProvider_Null_DoesNotReplaceExistingContext()
    {
        using var provider = new CollectingLoggerProvider();
        var logger = provider.CreateLogger("category");
        using (logger.BeginScope("scope"))
        {
            var exception = Assert.Throws<ArgumentNullException>(() => provider.SetScopeProvider(null!));
            Assert.Equal("scopeProvider", exception.ParamName);
            logger.LogInformation("message");
        }
        Assert.Equal(new object?[] { "scope" }, Assert.Single(provider.Entries).Scopes);
    }

    [Fact]
    public void Dispose_PreservesCollectedEntries()
    {
        var provider = new CollectingLoggerProvider();
        provider.CreateLogger("category").LogInformation("message");
        provider.Dispose();
        Assert.Equal("message", Assert.Single(provider.Entries).Message);
    }

    [Fact]
    public async Task Log_ConcurrentCategories_ShareOneStore()
    {
        using var provider = new CollectingLoggerProvider();
        var writers = Enumerable.Range(0, 8).Select(worker => Task.Run(() =>
        {
            var logger = provider.CreateLogger(worker.ToString());
            using (logger.BeginScope(worker))
            {
                for (var i = 0; i < 50; i++)
                    logger.LogInformation("{Id}", i);
            }
        }));
        await Task.WhenAll(writers);

        Assert.Equal(400, provider.Entries.Count);
        foreach (var group in provider.Entries.GroupBy(entry => entry.Category))
        {
            Assert.Equal(50, group.Count());
            Assert.All(group, entry => Assert.Equal(int.Parse(entry.Category), Assert.Single(entry.Scopes)));
        }
    }

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
        var secondLogger = provider.CreateLogger("second");
        var scopes = new LoggerExternalScopeProvider();
        provider.SetScopeProvider(scopes);

        using (scopes.Push("external"))
        {
            logger.LogInformation("category message");
            secondLogger.LogInformation("second message");
        }

        using (logger.BeginScope("logger scope"))
        {
            secondLogger.LogInformation("shared message");
        }

        Assert.Equal(new object?[] { "external" }, provider.Entries[0].Scopes);
        Assert.Equal(new object?[] { "external" }, provider.Entries[1].Scopes);
        Assert.Equal(new object?[] { "logger scope" }, provider.Entries[2].Scopes);
    }

    [Fact]
    public void Clear_RemovesAllCategoriesAndPreservesEarlierSnapshot()
    {
        using var provider = new CollectingLoggerProvider();
        var first = provider.CreateLogger("first");
        var second = provider.CreateLogger("second");
        first.LogInformation("first message");
        second.LogInformation("second message");
        var snapshot = provider.Entries;

        provider.Clear();

        Assert.Empty(provider.Entries);
        Assert.Equal(2, snapshot.Count);
        first.LogInformation("after clear");
        Assert.Equal("after clear", Assert.Single(provider.Entries).Message);
        Assert.Equal(2, snapshot.Count);
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
