using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FclEx.Serilog.Extensions.LoggerConfigurationExtensions;

public class ApplyMicrosoftLoggingFilterTests
{
    [Fact]
    public void DefaultLevel_None_FiltersFatal()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Logging:LogLevel:Default"] = "None"
            })
            .Build();
        var (logger, sink) = CreateLogger(config);
        using (logger)
        {
            logger.Fatal("must be disabled");
            Assert.Empty(sink.Events);
        }
    }

    [Theory]
    [InlineData("MYAPP", "myapp.Service", true)]
    [InlineData("MyApp.*.Worker", "MyApp.Background.Worker", true)]
    [InlineData("MyApp.*.Worker", "myapp.background.WORKER", true)]
    [InlineData("MyApp.*.Worker", "MyApp..Worker", true)]
    [InlineData("MyApp.*.Worker", "MyApp.Background.Worker.Other", false)]
    [InlineData("MyApp.*.Worker", "Other.Background.Worker", false)]
    [InlineData("*Worker", "MyApp.Background.Worker", true)]
    [InlineData("MyApp.*", "MyApp.Service", true)]
    [InlineData("*", "Any.Category", true)]
    [InlineData("Ab*Bc", "Abc", true)]
    public void CategoryOverride_UsesMicrosoftMatchingRules(string categoryRule, string category, bool matches)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Logging:LogLevel:Default"] = "Information",
                [$"Logging:LogLevel:{categoryRule}"] = "Error"
            })
            .Build();
        using var microsoftFactory = LoggerFactory.Create(builder => builder
            .AddConfiguration(config.GetSection("Logging"))
            .AddProvider(new global::Serilog.Extensions.Logging.SerilogLoggerProvider(
                new LoggerConfiguration().MinimumLevel.Verbose().CreateLogger(), dispose: true)));
        Assert.Equal(!matches, microsoftFactory.CreateLogger(category).IsEnabled(LogLevel.Warning));

        var (logger, sink) = CreateLogger(config);
        using (logger)
        {
            logger.ForContext("SourceContext", category).Warning("must be filtered");
            Assert.Equal(matches ? 0 : 1, sink.Events.Count);
        }
    }

    [Fact]
    public void CategoryOverride_EqualLengthMatches_UsesLastRule()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Logging:LogLevel:Default"] = "Error",
                ["Logging:LogLevel:*Work"] = "Warning",
                ["Logging:LogLevel:App.*"] = "Error"
            })
            .Build();
        using var microsoftFactory = LoggerFactory.Create(builder => builder
            .AddConfiguration(config.GetSection("Logging"))
            .AddProvider(new global::Serilog.Extensions.Logging.SerilogLoggerProvider(
                new LoggerConfiguration().MinimumLevel.Verbose().CreateLogger(), dispose: true)));
        Assert.True(microsoftFactory.CreateLogger("App.Work").IsEnabled(LogLevel.Warning));

        var (logger, sink) = CreateLogger(config);
        using (logger)
        {
            logger.ForContext("SourceContext", "App.Work").Warning("must be recorded");
            Assert.Single(sink.Events);
        }
    }

    [Fact]
    public void CategoryOverride_MultipleWildcards_Throws()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Logging:LogLevel:MyApp.*.*"] = "Warning"
            })
            .Build();

        Assert.Throws<InvalidOperationException>(() => CreateLogger(config));
    }

    [Fact]
    public void DisposedLogger_DoesNotKeepConfigurationOrSinkAlive()
    {
        var references = CreateDisposedReferences();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.False(references.Configuration.IsAlive);
        Assert.False(references.Sink.IsAlive);
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static (WeakReference Configuration, WeakReference Sink) CreateDisposedReferences()
    {
        var config = new ConfigurationBuilder().Build();
        var sink = new CollectingSink();
        var loggerConfiguration = new LoggerConfiguration()
            .ApplyMicrosoftLoggingFilter(config)
            .WriteTo.Sink(sink);
        using var logger = loggerConfiguration.CreateLogger();
        logger.Information("populate category cache");
        return (new WeakReference(loggerConfiguration), new WeakReference(sink));
    }

    [Fact]
    public void CategoryCache_UsesItsOwnConfigurationSnapshot()
    {
        var errorConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Logging:LogLevel:Default"] = "Error"
            })
            .Build();
        var warningConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Logging:LogLevel:Default"] = "Warning"
            })
            .Build();
        var (errorLogger, errorSink) = CreateLogger(errorConfig);
        var (warningLogger, warningSink) = CreateLogger(warningConfig);
        using (errorLogger)
        using (warningLogger)
        {
            errorLogger.ForContext("SourceContext", "MyApp.Service").Warning("must be filtered");
            warningLogger.ForContext("SourceContext", "MyApp.Service").Warning("must be recorded");
            errorLogger.ForContext("SourceContext", "myapp.service").Error("must be recorded");

            Assert.Equal(LogEventLevel.Error, Assert.Single(errorSink.Events).Level);
            Assert.Equal(LogEventLevel.Warning, Assert.Single(warningSink.Events).Level);
        }
    }

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
