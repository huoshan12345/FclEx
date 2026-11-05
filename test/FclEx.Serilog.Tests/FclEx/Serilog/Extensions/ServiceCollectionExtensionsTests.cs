using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;

namespace FclEx.Serilog.Extensions;

[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
public class ServiceCollectionExtensionsTests
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
    public void AddSerilog_AppliesLoggingBuilderConfiguredInsideCallback(bool useBuilderAlias)
    {
        var sink = new CollectingSink();
        using var provider = new ServiceCollection().AddSerilog((logger, configuration) =>
        {
            logger.WriteTo.Sink(sink);
            if (useBuilderAlias)
                configuration.ConfigureLoggingBuilder(builder => builder.SetMinimumLevel(LogLevel.Error));
            else
                configuration.ConfigureLogging(builder => builder.SetMinimumLevel(LogLevel.Error));
        }).BuildServiceProvider();
        var logger = provider.GetRequiredService<ILogger<ServiceCollectionExtensionsTests>>();

        logger.LogInformation("must be filtered");
        logger.LogError("must be recorded");

        Assert.Equal(LogEventLevel.Error, Assert.Single(sink.Events).Level);
    }

    [Fact]
    public void AddSerilog_LoggingBuilderFailure_DisposesCreatedSink()
    {
        var exception = new InvalidOperationException("logging configuration failed");
        var sink = new Mock<ILogEventSink>();
        var disposable = sink.As<IDisposable>();
        var options = new SerilogConfiguration().WriteTo(sink.Object)
            .ConfigureLogging(builder => throw exception);

        var actual = Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddSerilog(options));

        Assert.Same(exception, actual);
        disposable.Verify(m => m.Dispose(), Times.Once);
    }

    [Fact]
    public void AddSerilog_LoggingBuilderActions_RunInRegistrationOrder()
    {
        var calls = new List<int>();
        var options = new SerilogConfiguration().ConfigureLoggingBuilder(builder => calls.Add(1));
        options.Configure((logger, configuration) => configuration.ConfigureLogging(builder => calls.Add(2)));
        using var provider = new ServiceCollection().AddSerilog(options).BuildServiceProvider();

        Assert.Equal(new[] { 1, 2 }, calls);
    }

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
