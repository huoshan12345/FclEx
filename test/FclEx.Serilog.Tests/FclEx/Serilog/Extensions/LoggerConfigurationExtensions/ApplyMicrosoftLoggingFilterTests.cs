using Microsoft.Extensions.Configuration;

namespace FclEx.Serilog.Extensions.LoggerConfigurationExtensions;

public class ApplyMicrosoftLoggingFilterTests
{
    private static (Logger logger, CollectingSink sink) CreateLogger(IConfiguration config) 
    {
        var sink = new CollectingSink();

        var logger = new LoggerConfiguration()
            .ApplyMicrosoftLoggingFilter(config)
            .WriteTo.Sink(sink)
            .CreateLogger();

        return (logger, sink);
    }

    [Fact]
    public void DefaultLevel_Warning_FiltersInformation()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Logging:LogLevel:Default"] = "Warning"
            })
            .Build();

        var (logger, sink) = CreateLogger(config);

        logger.Information("info");
        logger.Warning("warn");

        Assert.Single(sink.Events);
        Assert.Equal(LogEventLevel.Warning, sink.Events.First().Level);
    }

    [Fact]
    public void CategoryOverride_AllowsLowerLevel()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Logging:LogLevel:Default"] = "Warning",
                ["Logging:LogLevel:MyApp"] = "Debug"
            })
            .Build();

        var (logger, sink) = CreateLogger(config);

        logger.ForContext("SourceContext", "MyApp.Service")
              .Debug("debug");

        Assert.Single(sink.Events);
    }

    [Fact]
    public void CategoryOverride_FiltersLowerLevels()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Logging:LogLevel:Default"] = "Information",
                ["Logging:LogLevel:MyApp"] = "Error"
            })
            .Build();

        var (logger, sink) = CreateLogger(config);

        var log = logger.ForContext("SourceContext", "MyApp.Service");

        log.Warning("warn");
        log.Error("error");

        Assert.Single(sink.Events);
        Assert.Equal(LogEventLevel.Error, sink.Events.First().Level);
    }

    [Fact]
    public void LongestPrefixRule_Works()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Logging:LogLevel:Default"] = "Warning",
                ["Logging:LogLevel:MyApp"] = "Information",
                ["Logging:LogLevel:MyApp.Service"] = "Error"
            })
            .Build();

        var (logger, sink) = CreateLogger(config);

        var log = logger.ForContext("SourceContext", "MyApp.Service");

        log.Warning("warn");
        log.Error("error");

        Assert.Single(sink.Events);
        Assert.Equal(LogEventLevel.Error, sink.Events.First().Level);
    }

    [Fact]
    public void NoLoggingConfig_DoesNotThrow()
    {
        var config = new ConfigurationBuilder().Build();

        var ex = Record.Exception(() =>
        {
            CreateLogger(config);
        });

        Assert.Null(ex);
    }
}
