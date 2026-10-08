using Microsoft.Extensions.Logging.Abstractions;

namespace FclEx.Logging;

public class PropertiesLoggerTests
{
    [Fact]
    public void Constructor_CopiesDefinitionsWithoutEvaluatingLazyFactories()
    {
        var collector = new CollectingLogger();
        var fixedProperties = new LoggerProperty[] { new("Fixed", 1) };
        var calls = 0;
        var lazyProperties = new LazyLoggerProperty[] { new("Lazy", () => ++calls) };
        var logger = new PropertiesLogger(collector, fixedProperties, lazyProperties);
        fixedProperties[0] = new("Changed", 2);
        lazyProperties[0] = new("ChangedLazy", () => 99);

        Assert.Equal(0, calls);
        logger.LogInformation("message");

        var properties = Assert.Single(collector.Entries).Scopes.SelectMany(scope =>
            Assert.IsAssignableFrom<IEnumerable<KeyValuePair<string, object?>>>(scope)).ToArray();
        Assert.Equal(new[] { "Fixed", "Lazy" }, properties.Select(property => property.Key));
        Assert.Equal(new object?[] { 1, 1 }, properties.Select(property => property.Value));
        Assert.Equal(1, calls);
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var error = Assert.Throws<ArgumentNullException>(() => new PropertiesLogger(null!));
        Assert.Equal("logger", error.ParamName);
    }

    [Fact]
    public void Log_FactoryFilteredLevel_DoesNotEvaluateLazyProperties()
    {
        using var factory = LoggerFactory.Create(builder => builder.AddCollecting().SetMinimumLevel(LogLevel.Warning));
        var logger = new PropertiesLogger(factory.CreateLogger("test"),
            lazyProperties: new LazyLoggerProperty[] { new("Value", () => throw new InvalidOperationException()) });

        Assert.False(logger.IsEnabled(LogLevel.Information));
        logger.LogInformation("discarded");
    }

    [Fact]
    public void Log_LazyProperties_AreEvaluatedOncePerEntryAndRemainStable()
    {
        var collector = new CollectingLogger();
        var calls = 0;
        var logger = new PropertiesLogger(collector,
            lazyProperties: new LazyLoggerProperty[] { new("Counter", () => ++calls) });

        logger.LogInformation("first");
        logger.LogInformation("second");

        Assert.Equal(2, calls);
        for (var i = 0; i < 2; i++)
        {
            var properties = Assert.IsAssignableFrom<IEnumerable<KeyValuePair<string, object?>>>(
                Assert.Single(collector.Entries[i].Scopes));
            Assert.Equal(i + 1, Assert.Single(properties).Value);
            Assert.Equal(i + 1, Assert.Single(properties).Value);
        }
        Assert.Equal(2, calls);
    }

    [Fact]
    public void Log_DisabledLevel_DoesNotEvaluateLazyProperties()
    {
        var logger = new PropertiesLogger(NullLogger.Instance,
            lazyProperties: new LazyLoggerProperty[] { new("Value", () => throw new InvalidOperationException()) });

        Assert.False(logger.IsEnabled(LogLevel.Information));
        logger.LogInformation("discarded");
    }

    [Fact]
    public void Log_MultipleProviders_SeeTheSameLazyValue()
    {
        using var first = new CollectingLoggerProvider();
        using var second = new CollectingLoggerProvider();
        using var factory = LoggerFactory.Create(builder => builder.AddProvider(first).AddProvider(second));
        var calls = 0;
        var logger = new PropertiesLogger(factory.CreateLogger("test"),
            lazyProperties: new LazyLoggerProperty[] { new("Counter", () => ++calls) });

        logger.LogInformation("message");

        foreach (var provider in new[] { first, second })
        {
            var properties = Assert.IsAssignableFrom<IEnumerable<KeyValuePair<string, object?>>>(
                Assert.Single(Assert.Single(provider.Entries).Scopes));
            Assert.Equal(1, Assert.Single(properties).Value);
        }
        Assert.Equal(1, calls);
    }

    [Fact]
    public void Log_LazyFactoryThrows_RestoresExistingScopesAndDoesNotWrite()
    {
        var collector = new CollectingLogger();
        var error = new InvalidOperationException("factory failed");
        var logger = new PropertiesLogger(collector, new LoggerProperty[] { new("Fixed", 1) },
            new LazyLoggerProperty[] { new("Lazy", () => throw error) });
        using (collector.BeginScope("outer"))
        {
            Assert.Same(error, Assert.Throws<InvalidOperationException>(() => logger.LogInformation("failed")));
            collector.LogInformation("after failure");
        }

        Assert.Equal(new object?[] { "outer" }, Assert.Single(collector.Entries).Scopes);
    }

    [Fact]
    public void Log_NestedWrappers_PreservePropertiesAndRestoreScopes()
    {
        var collector = new CollectingLogger();
        var calls = 0;
        var inner = new PropertiesLogger(collector, new LoggerProperty[] { new("Fixed", 1) },
            new LazyLoggerProperty[] { new("Lazy", () => ++calls) });
        var outer = inner.With("Other", 2);

        outer.LogInformation("wrapped");
        collector.LogInformation("unwrapped");

        var properties = collector.Entries[0].Scopes.SelectMany(scope =>
            Assert.IsAssignableFrom<IEnumerable<KeyValuePair<string, object?>>>(scope)).ToArray();
        Assert.Equal(new[] { "Fixed", "Other", "Lazy" }, properties.Select(property => property.Key));
        Assert.Equal(new object?[] { 1, 2, 1 }, properties.Select(property => property.Value));
        Assert.Equal(1, calls);
        Assert.Empty(collector.Entries[1].Scopes);
    }
}
