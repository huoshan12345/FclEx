using Microsoft.Extensions.Logging;

namespace FclEx.Serilog;

public class SerilogConfigurationTests
{
    [Fact]
    public void Configure_NullAction_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new SerilogConfiguration().Configure(null!));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CreateLogger_PreservesExceptionWhenOnlyPlaceholderNameMatches(bool formatException)
    {
        var sink = new CollectingSink();
        var configuration = new SerilogConfiguration { FormatException = formatException };
        using var logger = (Logger)configuration.WriteTo(sink).CreateLogger();
        var exception = new SimpleException("Method");

        logger.Error(exception, "Failed to execute {Method}", "Save");

        Assert.Same(exception, Assert.Single(sink.Events).GetOriginalException());
    }

    [Fact]
    public void CreateLogger_Dispose_DisposesWrappedSink()
    {
        var sink = new Mock<ILogEventSink>();
        var disposable = sink.As<IDisposable>();
        Logger logger = new SerilogConfiguration().WriteTo(sink.Object).CreateLogger();

        logger.Dispose();

        disposable.Verify(m => m.Dispose(), Times.Once);
    }

    [Fact]
    public void CreateLogger_ConfigurationFailure_DisposesRegisteredSink()
    {
        var exception = new InvalidOperationException("configuration failed");
        var sink = new Mock<ILogEventSink>();
        var disposable = sink.As<IDisposable>();
        var configuration = new SerilogConfiguration().Configure(logger =>
        {
            logger.WriteTo.Sink(sink.Object);
            throw exception;
        });

        var actual = Assert.Throws<InvalidOperationException>(() => configuration.CreateLogger());

        Assert.Same(exception, actual);
        disposable.Verify(m => m.Dispose(), Times.Once);
    }

    [Fact]
    public void CreateLogger_ConfigurationAndCleanupFail_PreservesBothErrors()
    {
        var configurationError = new InvalidOperationException("configuration failed");
        var cleanupError = new IOException("cleanup failed");
        var sink = new Mock<ILogEventSink>();
        sink.As<IDisposable>().Setup(m => m.Dispose()).Throws(cleanupError);
        var configuration = new SerilogConfiguration().Configure(logger =>
        {
            logger.WriteTo.Sink(sink.Object);
            throw configurationError;
        });

        var actual = Assert.Throws<AggregateException>(() => configuration.CreateLogger());

        Assert.Equal(new Exception[] { configurationError, cleanupError }, actual.InnerExceptions);
    }

#if NET6_0_OR_GREATER
    [Fact]
    public void CreateLogger_ConfigurationFailure_DisposesAsyncOnlySink()
    {
        var exception = new InvalidOperationException("configuration failed");
        var sink = new Mock<ILogEventSink>();
        var asynchronous = sink.As<IAsyncDisposable>();
        asynchronous.Setup(m => m.DisposeAsync()).Returns(ValueTask.CompletedTask);
        var configuration = new SerilogConfiguration().Configure(logger =>
        {
            logger.WriteTo.Sink(sink.Object);
            throw exception;
        });

        var actual = Assert.Throws<InvalidOperationException>(() => configuration.CreateLogger());

        Assert.Same(exception, actual);
        asynchronous.Verify(m => m.DisposeAsync(), Times.Once);
    }
#endif

    [Theory]
    [InlineData(LogEventLevel.Information, LogLevel.Error, false)]
    [InlineData(LogEventLevel.Error, LogLevel.Warning, false)]
    [InlineData(LogEventLevel.Error, LogLevel.Error, true)]
    [InlineData(LogEventLevel.Fatal, LogLevel.Critical, true)]
    [InlineData(LogEventLevel.Error, LogLevel.None, false)]
    public void CreateLogger_RequiresOriginalAndExceptionLevelsToPassMinimum(LogEventLevel originalLevel, LogLevel exceptionLevel, bool shouldLog)
    {
        var sink = new CollectingSink();
        var configuration = new SerilogConfiguration { MinimumLevel = LogEventLevel.Error };
        using var logger = (Logger)configuration.WriteTo(sink).CreateLogger();

        logger.Write(originalLevel, new LogException("failure", exceptionLevel), "message");

        Assert.Equal(shouldLog ? 1 : 0, sink.Events.Count);
        if (shouldLog)
            Assert.Equal(exceptionLevel.ToSerilogLevel(), sink.Events[0].Level);
    }

    [Theory]
    [InlineData(LogEventLevel.Verbose, LogEventLevel.Error, false)]
    [InlineData(LogEventLevel.Error, LogEventLevel.Debug, true)]
    public void CreateLogger_RechecksCategoryMinimum(LogEventLevel minimumLevel, LogEventLevel categoryMinimum, bool shouldLog)
    {
        var sink = new CollectingSink();
        var configuration = new SerilogConfiguration { MinimumLevel = minimumLevel };
        configuration.Configure(logger => logger.MinimumLevel.Override("MyApp", categoryMinimum));
        using var logger = (Logger)configuration.WriteTo(sink).CreateLogger();

        logger.ForContext(Constants.SourceContext, "MyApp.Service")
            .Error(new LogException("failure", LogLevel.Warning), "message");

        Assert.Equal(shouldLog ? 1 : 0, sink.Events.Count);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("MyApp.Service")]
    public void CreateLogger_RechecksCurrentLevelSwitch(string? source)
    {
        var sink = new CollectingSink();
        var levelSwitch = new LoggingLevelSwitch(LogEventLevel.Warning);
        var configuration = new SerilogConfiguration();
        configuration.Configure(logger => logger.MinimumLevel.ControlledBy(levelSwitch));
        using var logger = (Logger)configuration.WriteTo(sink).CreateLogger();
        var sourceLogger = source is null ? logger : logger.ForContext(Constants.SourceContext, source);

        sourceLogger.Error(new LogException("first", LogLevel.Warning), "message");
        levelSwitch.MinimumLevel = LogEventLevel.Error;
        sourceLogger.Error(new LogException("second", LogLevel.Warning), "message");
        levelSwitch.MinimumLevel = LogEventLevel.Verbose;
        sourceLogger.Error(new LogException("third", LogLevel.Warning), "message");

        Assert.Equal(2, sink.Events.Count);
        Assert.All(sink.Events, logEvent => Assert.Equal(LogEventLevel.Warning, logEvent.Level));
    }

    [Fact]
    public void CreateLogger_CachedSourceUsesCurrentCategoryLevelSwitch()
    {
        var sink = new CollectingSink();
        var levelSwitch = new LoggingLevelSwitch(LogEventLevel.Warning);
        var configuration = new SerilogConfiguration();
        configuration.Configure(logger => logger.MinimumLevel.Override("MyApp", levelSwitch));
        using var logger = (Logger)configuration.WriteTo(sink).CreateLogger();
        var sourceLogger = logger.ForContext(Constants.SourceContext, "MyApp.Service");

        sourceLogger.Error(new LogException("first", LogLevel.Warning), "message");
        levelSwitch.MinimumLevel = LogEventLevel.Error;
        sourceLogger.Error(new LogException("second", LogLevel.Warning), "message");
        levelSwitch.MinimumLevel = LogEventLevel.Warning;
        sourceLogger.Error(new LogException("third", LogLevel.Warning), "message");

        Assert.Equal(2, sink.Events.Count);
    }

    [Fact]
    public void CreateLogger_SourceCachesUseTheirOwnLoggerConfiguration()
    {
        var source = nameof(CreateLogger_SourceCachesUseTheirOwnLoggerConfiguration);
        var errorSink = new CollectingSink();
        var warningSink = new CollectingSink();
        using var errorLogger = (Logger)new SerilogConfiguration { MinimumLevel = LogEventLevel.Error }
            .WriteTo(errorSink).CreateLogger();
        using var warningLogger = (Logger)new SerilogConfiguration { MinimumLevel = LogEventLevel.Warning }
            .WriteTo(warningSink).CreateLogger();

        errorLogger.ForContext(Constants.SourceContext, source)
            .Error(new LogException("failure", LogLevel.Warning), "message");
        warningLogger.ForContext(Constants.SourceContext, source)
            .Error(new LogException("failure", LogLevel.Warning), "message");

        Assert.Empty(errorSink.Events);
        Assert.Single(warningSink.Events);
    }

    [Fact]
    public void CreateLogger_FiltersSeeExceptionLevel()
    {
        var sink = new CollectingSink();
        var configuration = new SerilogConfiguration();
        configuration.Exclude(logEvent => logEvent.Level < LogEventLevel.Error);
        using var logger = (Logger)configuration.WriteTo(sink).CreateLogger();

        logger.Error(new LogException("failure", LogLevel.Warning), "message");

        Assert.Empty(sink.Events);
    }

    [Fact]
    public void CreateLogger_RechecksMinimumConfiguredByAction()
    {
        var sink = new CollectingSink();
        var configuration = new SerilogConfiguration();
        configuration.Configure(logger => logger.MinimumLevel.Error());
        using var logger = (Logger)configuration.WriteTo(sink).CreateLogger();

        logger.Error(new LogException("failure", LogLevel.Warning), "message");

        Assert.Empty(sink.Events);
    }

    [Fact]
    public void CreateLogger_SinksFilterUsingExceptionLevel()
    {
        var warningSink = new CollectingSink();
        var errorSink = new CollectingSink();
        var configuration = new SerilogConfiguration();
        configuration.Configure(logger => logger.WriteTo
            .Sink(warningSink, restrictedToMinimumLevel: LogEventLevel.Warning)
            .WriteTo.Sink(errorSink, restrictedToMinimumLevel: LogEventLevel.Error));
        using var logger = (Logger)configuration.CreateLogger();

        logger.Error(new LogException("failure", LogLevel.Warning), "message");

        Assert.Single(warningSink.Events);
        Assert.Empty(errorSink.Events);
    }
}
