using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FclEx.Serilog.Extensions;

public class LoggerPropertyScopeExtensionsTests
{
    // These two tests inspect retention in the shared cache; eviction must not race the reuse assertion.
    [Fact(DisableParallelization = true)]
    public void Push_RepeatedName_ReusesPrefixedString()
    {
        var names = new List<string>();
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger>();
        logger.Setup(value => value.BeginScope(It.IsAny<KeyValuePair<string, object?>[]>()))
            .Callback<KeyValuePair<string, object?>[]>(state => names.Add(Assert.Single(state).Key));
        var name = "CachedProperty_" + Guid.NewGuid().ToString("N");

        using (var first = global::FclEx.Serilog.LoggerExtensions.Properties(logger.Object, name, null, true)) { }
        using (var second = global::FclEx.Serilog.LoggerExtensions.Properties(logger.Object, name, null, true)) { }

        Assert.Equal("@" + name, names[0]);
        Assert.Same(names[0], names[1]);
    }

    [Fact(DisableParallelization = true)]
    public void DisposeScope_EvictedDynamicPropertyNameCanBeCollected()
    {
        var reference = CreateDisposedPropertyNameReference();

        // A bounded cache may retain recent names; churn beyond its 1024-entry capacity must release old names.
        for (var i = 0; i < 2048; i++)
        {
            using var scope = global::FclEx.Serilog.LoggerExtensions.Properties(
                Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance, "Eviction_" + Guid.NewGuid().ToString("N"),
                null, destructureObjects: true);
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.False(reference.IsAlive);
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static WeakReference CreateDisposedPropertyNameReference()
    {
        var name = "DynamicProperty_" + Guid.NewGuid().ToString("N");
        using var scope = global::FclEx.Serilog.LoggerExtensions.Properties(
            Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance, name, new object(), destructureObjects: true);
        return new WeakReference(name);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Properties_UsesSerilogDestructuringAndRestoresScope(bool destructureObjects)
    {
        var sink = new CollectingSink();
        var services = new ServiceCollection();
        services.AddSerilog(configuration => configuration.WriteTo.Sink(sink));
        using var provider = services.BuildServiceProvider();
        var logger = provider.GetRequiredService<ILogger<LoggerPropertyScopeExtensionsTests>>();

        using (logger.Properties("Payload", new Payload("value"), destructureObjects))
            logger.LogInformation("scoped");
        logger.LogInformation("after");

        var value = sink.Events[0].Properties["Payload"];
        if (destructureObjects)
        {
            var structured = Assert.IsType<StructureValue>(value);
            var property = Assert.Single(structured.Properties);
            Assert.Equal("Name", property.Name);
            Assert.Equal("value", Assert.IsType<ScalarValue>(property.Value).Value);
        }
        else
        {
            Assert.IsType<ScalarValue>(value);
        }
        Assert.False(sink.Events[1].Properties.ContainsKey("Payload"));
    }

    [Fact]
    public void Push_ExistingDestructuringPrefix_IsNotDuplicated()
    {
        var sink = new CollectingSink();
        var services = new ServiceCollection();
        services.AddSerilog(configuration => configuration.WriteTo.Sink(sink));
        using var provider = services.BuildServiceProvider();
        var logger = provider.GetRequiredService<ILogger<LoggerPropertyScopeExtensionsTests>>();
        using (var scope = logger.Properties())
        {
            Assert.Same(scope, scope.Push("@Payload", new Payload("value"), true));
            logger.LogInformation("message");
        }

        Assert.IsType<StructureValue>(Assert.Single(sink.Events).Properties["Payload"]);
    }

    public sealed record Payload(string Name);
}
