using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FclEx.Serilog.Extensions;

public class LoggerPropertyScopeExtensionsTests
{
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
