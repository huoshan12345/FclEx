using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FclEx.Serilog.Extensions.LoggerConfigurationExtensions;

public class ApplyMicrosoftLoggingFilterTests
{
    private static (Logger logger, CollectingSink sink) CreateLogger(IConfiguration config)
    {
        var sink = new CollectingSink();

        var logger = new LoggerConfiguration()
            .ApplyMicrosoftLoggingFilter(config.GetSection("Logging"))
            .MinimumLevel.Verbose()
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

    [Fact]
    public void InvalidLogLevel_Throw()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Logging:LogLevel:Default"] = "InvalidLevel"
            })
            .Build();

        var ex = Assert.Throws<InvalidOperationException>(() => CreateLogger(config));
        Assert.Contains("Configuration value 'InvalidLevel' is not supported.", ex.Message);
    }

    [Fact]
    public void LogWithoutSourceContext_UsesDefaultLevel()
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
    public void MicrosoftNamespaceOverride_Works()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Logging:LogLevel:Default"] = "Information",
                ["Logging:LogLevel:Microsoft"] = "Warning"
            })
            .Build();

        var (logger, sink) = CreateLogger(config);

        var log = logger.ForContext("SourceContext", "Microsoft.AspNetCore.Hosting");

        log.Information("info");
        log.Warning("warn");

        Assert.Single(sink.Events);
        Assert.Equal(LogEventLevel.Warning, sink.Events.First().Level);
    }

    public static readonly TheoryData<LogLevel, LogEventLevel, bool> LogLevelMatrix = Enum.GetValues<LogLevel>()
        .CrossJoin(m => Enum.GetValues<LogEventLevel>())
        .Select(m => (m.Item1, m.Item2, m.Item1.ToSerilogLevel() <= m.Item2))
        .ToTheoryData();

    [Theory]
    [MemberData(nameof(LogLevelMatrix))]
    public void DefaultLevel_FilterWorks(LogLevel configuredLevel, LogEventLevel eventLevel, bool shouldLog)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Logging:LogLevel:Default"] = configuredLevel.ToString()
            })
            .Build();

        var (logger, sink) = CreateLogger(config);

        logger.Write(eventLevel, "test");

        if (shouldLog)
        {
            Assert.Single(sink.Events);
        }
        else
        {
            Assert.Empty(sink.Events);
        }
    }
}
