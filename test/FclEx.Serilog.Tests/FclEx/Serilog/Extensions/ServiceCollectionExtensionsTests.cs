using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;

namespace FclEx.Serilog.Extensions;

[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
public class ServiceCollectionExtensionsTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AddSerilog_Test(bool formatException)
    {
        var sink = new CollectingSink();
        var provider = new ServiceCollection()
            .AddSerilog((m, n) =>
            {
                n.WriteTo(sink)
                    .Enrich(new LogEnricher(nameof(AddSerilog_Test)))
                    .FormatException(formatException);
            })
            .BuildServiceProvider();

        var logger = provider.GetService<Microsoft.Extensions.Logging.ILogger>();
        Assert.NotNull(logger);

        var loggerFactory = provider.GetService<ILoggerFactory>();
        Assert.NotNull(loggerFactory);

        var typedLogger = provider.GetService<ILogger<ServiceCollectionExtensionsTests>>();
        Assert.NotNull(typedLogger);

        var serilogLogger = provider.GetService<global::Serilog.ILogger>();
        Assert.NotNull(serilogLogger);

        serilogLogger.Information(new LogException("exception", LogLevel.Warning).SetStackTrace(), "message");

        var logEvent = Assert.Single(sink.Events);
        Assert.Equal(LogEventLevel.Warning, logEvent.Level);
        Assert.Equal("exception", logEvent.Exception?.Message);
        Assert.Equal("message", logEvent.MessageTemplate.Text);

        if (formatException)
        {
            Assert.IsType<FormattedException>(logEvent.Exception);
        }
        else
        {
            Assert.IsType<LogException>(logEvent.Exception);
        }
    }

    [Fact]
    public void LoggingScopeShouldNotReplacePropertyInLogEvent()
    {
        var sink = new CollectingSink();
        var services = new ServiceCollection();
        services.AddSerilog(m => m.WriteTo.Sink(sink));

        using var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<ServiceCollectionExtensionsTests>>();

        using (logger.BeginScope(new Dictionary<string, object?> { { "Value", 1 } }))
        using (logger.BeginScope(new Dictionary<string, object?> { { "Value", 2 } }))
        {
            logger.LogInformation("Value: {Value}", 3);
        }

        var logEvent = sink.Events.First();
        var value = (logEvent.Properties["Value"] as ScalarValue)?.Value;

        Assert.Equal(3, value);
    }
}
