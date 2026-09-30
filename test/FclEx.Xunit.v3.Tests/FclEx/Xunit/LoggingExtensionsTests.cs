using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FclEx.Xunit;

public class LoggingExtensionsTests
{
    [Fact]
    public void AddXunit_UnavailableOutputWithoutFallback_DropsMessages()
    {
        using var factory = LoggerFactory.Create(_ => { });
        var resolverCalls = 0;
        factory.AddXunit(() =>
        {
            resolverCalls++;
            return null!;
        }, consoleFallback: false);
        factory.CreateLogger("category").LogInformation("unavailable output");
        Assert.Equal(1, resolverCalls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AddXunit_Builder_WritesToExplicitOutputAndRespectsFilters(bool useResolver)
    {
        var output = new RecordingOutputHelper();
        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Warning);
            Assert.Same(builder, useResolver
                ? builder.AddXunit(() => output, consoleFallback: false)
                : builder.AddXunit(output, consoleFallback: false));
        });
        using var serviceProvider = services.BuildServiceProvider();
        Assert.IsType<XunitLoggerProvider>(Assert.Single(serviceProvider.GetServices<ILoggerProvider>()));
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("category");
        logger.LogInformation("filtered");
        logger.LogWarning("collected");

        Assert.Contains("[category][Warning]collected", output.Output);
        Assert.DoesNotContain("filtered", output.Output);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AddXunit_Factory_WritesToExplicitOutput(bool useResolver)
    {
        var output = new RecordingOutputHelper();
        using var factory = LoggerFactory.Create(_ => { });
        var logger = factory.CreateLogger("category");
        Assert.Same(factory, useResolver
            ? factory.AddXunit(() => output, consoleFallback: false)
            : factory.AddXunit(output, consoleFallback: false));
        logger.LogInformation("message");
        Assert.Contains("[category][Information]message", output.Output);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AddXunit_UsesCurrentTestOutput(bool useBuilder)
    {
        using var factory = LoggerFactory.Create(builder =>
        {
            if (useBuilder)
                builder.AddXunit(consoleFallback: false);
        });
        if (!useBuilder)
            factory.AddXunit(consoleFallback: false);

        var message = Guid.NewGuid().ToString();
        factory.CreateLogger("category").LogInformation("{Message}", message);
        Assert.Contains(message, TestContext.Current.TestOutputHelper!.Output);
    }

    [Fact]
    public void AddXunit_Resolver_IsInvokedForEachMessage()
    {
        var first = new RecordingOutputHelper();
        var second = new RecordingOutputHelper();
        var current = first;
        using var factory = LoggerFactory.Create(_ => { });
        factory.AddXunit(() => current, consoleFallback: false);
        var logger = factory.CreateLogger("category");
        logger.LogInformation("first");
        current = second;
        logger.LogInformation("second");

        Assert.Contains("first", first.Output);
        Assert.DoesNotContain("second", first.Output);
        Assert.Contains("second", second.Output);
        Assert.DoesNotContain("first", second.Output);
    }

    private sealed class RecordingOutputHelper : ITestOutputHelper
    {
        private readonly StringBuilder _output = new();
        public string Output => _output.ToString();
        public void Write(string message) => _output.Append(message);
        public void Write(string format, params object[] args) => Write(string.Format(format, args));
        public void WriteLine(string message) => _output.AppendLine(message);
        public void WriteLine(string format, params object[] args) => WriteLine(string.Format(format, args));
    }
}
