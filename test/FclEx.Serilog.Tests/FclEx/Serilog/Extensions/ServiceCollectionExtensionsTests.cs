using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;

namespace FclEx.Serilog.Extensions;

[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
public partial class ServiceCollectionExtensionsTests
{
    [Theory]
    [InlineData(LogLevel.Warning, false)]
    [InlineData(LogLevel.Error, true)]
    public void AddSerilog_LogError_RequiresExceptionLevelToPassMinimum(LogLevel exceptionLevel, bool shouldLog)
    {
        var sink = new CollectingSink();
        var options = new SerilogConfiguration { MinimumLevel = LogEventLevel.Error };
        options.WriteTo(sink);
        using var provider = new ServiceCollection().AddSerilog(options).BuildServiceProvider();
        var logger = provider.GetRequiredService<ILogger<ServiceCollectionExtensionsTests>>();

        try
        {
            throw new LogException("inner failure", exceptionLevel);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "outer failure");
        }

        Assert.Equal(shouldLog ? 1 : 0, sink.Events.Count);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AddSerilog_AppliesConfigureLoggingBuilderBeforeRegistration(bool useBuilderAlias)
    {
        var sink = new CollectingSink();
        var options = new SerilogConfiguration().WriteTo(sink);
        if (useBuilderAlias)
            options.ConfigureLoggingBuilder(builder => builder.SetMinimumLevel(LogLevel.Error));
        else
            options.ConfigureLogging(builder => builder.SetMinimumLevel(LogLevel.Error));
        using var provider = new ServiceCollection().AddSerilog(options).BuildServiceProvider();
        var logger = provider.GetRequiredService<ILogger<ServiceCollectionExtensionsTests>>();

        logger.LogInformation("must be filtered");

        Assert.Empty(sink.Events);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AddSerilog_AppliesLoggingBuilderBeforeDeferredLoggerConfiguration(bool useBuilderAlias)
    {
        var sink = new CollectingSink();
        var calls = new List<string>();
        var options = new SerilogConfiguration().Configure(logger =>
        {
            calls.Add("logger");
            logger.WriteTo.Sink(sink);
        });
        Action<ILoggingBuilder> configureLogging = builder =>
        {
            calls.Add("logging builder");
            builder.SetMinimumLevel(LogLevel.Error);
        };
        if (useBuilderAlias)
            options.ConfigureLoggingBuilder(configureLogging);
        else
            options.ConfigureLogging(configureLogging);
        var services = new ServiceCollection().AddSerilog(options);
        Assert.Equal(new[] { "logging builder" }, calls);
        using var provider = services.BuildServiceProvider();
        var logger = provider.GetRequiredService<ILogger<ServiceCollectionExtensionsTests>>();

        logger.LogInformation("must be filtered");
        logger.LogError("must be recorded");

        Assert.Equal(LogEventLevel.Error, Assert.Single(sink.Events).Level);
        Assert.Equal(new[] { "logging builder", "logger" }, calls);
    }

    [Fact]
    public void AddSerilog_LoggingBuilderFailure_DoesNotCreateLogger()
    {
        var exception = new InvalidOperationException("logging configuration failed");
        var loggerCreations = 0;
        var options = new SerilogConfiguration().Configure(logger => loggerCreations++)
            .ConfigureLogging(builder => throw exception);

        var actual = Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddSerilog(options));

        Assert.Same(exception, actual);
        Assert.Equal(0, loggerCreations);
    }

    [Fact]
    public void AddSerilog_LoggingBuilderActions_RunInRegistrationOrder()
    {
        var calls = new List<int>();
        var options = new SerilogConfiguration().ConfigureLoggingBuilder(builder => calls.Add(1));
        options.ConfigureLogging(builder => calls.Add(2));
        using var provider = new ServiceCollection().AddSerilog(options).BuildServiceProvider();

        Assert.Equal(new[] { 1, 2 }, calls);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AddSerilog_Test(bool formatException)
    {
        var sink = new CollectingSink();
        var options = new SerilogConfiguration().AddCommonExcluders()
            .WriteTo(sink)
            .Enrich(new LogEnricher(nameof(AddSerilog_Test)))
            .FormatException(formatException);
        using var provider = new ServiceCollection()
            .AddSerilog(options)
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
