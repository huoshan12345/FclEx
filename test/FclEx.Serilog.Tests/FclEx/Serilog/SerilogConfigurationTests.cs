using Microsoft.Extensions.Logging;

namespace FclEx.Serilog;

public class SerilogConfigurationTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CreateLogger_MultipleSinks_NormalizeNestedLogExceptionOnce(bool formatException)
    {
        var levels = new List<LogEventLevel>();
        var exceptions = new List<Exception?>();
        var first = new Mock<ILogEventSink>();
        first.Setup(sink => sink.Emit(It.IsAny<LogEvent>())).Callback<LogEvent>(logEvent =>
        {
            levels.Add(logEvent.Level);
            exceptions.Add(logEvent.Exception);
        });
        var second = new Mock<ILogEventSink>();
        second.Setup(sink => sink.Emit(It.IsAny<LogEvent>())).Callback<LogEvent>(logEvent =>
        {
            levels.Add(logEvent.Level);
            exceptions.Add(logEvent.Exception);
        });
        var configuration = new SerilogConfiguration
        {
            MinimumLevel = LogEventLevel.Error,
            FormatException = formatException
        };
        using var logger = configuration.WriteTo(first.Object).WriteTo(second.Object).CreateLogger();
        var inner = new LogException("failure", LogLevel.Warning, new InvalidOperationException("failure"));
        var outer = new LogException("failure", LogLevel.Error, inner);

        logger.Error(outer, "failure");

        Assert.Equal(new[] { LogEventLevel.Error, LogEventLevel.Error }, levels);
        Assert.NotNull(exceptions[0]);
        Assert.Same(exceptions[0], exceptions[1]);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CreateLogger_MultipleSinks_FailureDoesNotPreventLaterSinks(bool formatException)
    {
        var failing = new Mock<ILogEventSink>();
        failing.Setup(sink => sink.Emit(It.IsAny<LogEvent>())).Throws(new IOException("sink failed"));
        var collecting = new CollectingSink();
        using var logger = new SerilogConfiguration { FormatException = formatException }
            .WriteTo(failing.Object).WriteTo(collecting).CreateLogger();

        logger.Error(new LogException("failure", LogLevel.Warning), "failure");

        failing.Verify(sink => sink.Emit(It.IsAny<LogEvent>()), Times.Once);
        Assert.Equal(LogEventLevel.Warning, Assert.Single(collecting.Events).Level);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CreateLogger_MultipleSinks_RestrictionsUseOuterExceptionLevel(bool formatException)
    {
        var unrestricted = new CollectingSink();
        var restricted = new CollectingSink();
        var switched = new CollectingSink();
        var levelSwitch = new LoggingLevelSwitch(LogEventLevel.Error);
        var configuration = new SerilogConfiguration { FormatException = formatException };
        configuration.Configure(logger => logger.WriteTo.Sink(unrestricted)
            .WriteTo.Sink(restricted, restrictedToMinimumLevel: LogEventLevel.Error)
            .WriteTo.Sink(switched, levelSwitch: levelSwitch));
        using var logger = configuration.CreateLogger();

        // The inner level must not replace the already-filtered outer level during fan-out.
        logger.Error(new LogException("failure", LogLevel.Error,
            new LogException("failure", LogLevel.Warning, new InvalidOperationException("failure"))), "failure");
        levelSwitch.MinimumLevel = LogEventLevel.Fatal;
        logger.Error(new LogException("failure", LogLevel.Warning,
            new LogException("failure", LogLevel.Error, new InvalidOperationException("failure"))), "failure");

        Assert.Equal(new[] { LogEventLevel.Error, LogEventLevel.Warning }, unrestricted.Events.Select(logEvent => logEvent.Level));
        Assert.Equal(LogEventLevel.Error, Assert.Single(restricted.Events).Level);
        Assert.Equal(LogEventLevel.Error, Assert.Single(switched.Events).Level);
    }

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

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void CreateLogger_Dispose_DisposesWrappedSinksOnce(int sinkCount)
    {
        var configuration = new SerilogConfiguration();
        var sinks = Enumerable.Range(0, sinkCount).Select(_ => new Mock<ILogEventSink>()).ToArray();
        foreach (var sink in sinks)
        {
            sink.As<IDisposable>();
            configuration.WriteTo(sink.Object);
        }
        var logger = configuration.CreateLogger();

        logger.Dispose();

        foreach (var sink in sinks)
            sink.As<IDisposable>().Verify(disposable => disposable.Dispose(), Times.Once);
    }

#if NET6_0_OR_GREATER
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task CreateLogger_DisposeAsync_DisposesWrappedSinksOnce(int asyncSinkCount)
    {
        var configuration = new SerilogConfiguration();
        var sinks = Enumerable.Range(0, asyncSinkCount).Select(_ => new Mock<ILogEventSink>()).ToArray();
        foreach (var sink in sinks)
        {
            sink.As<IAsyncDisposable>().Setup(disposable => disposable.DisposeAsync()).Returns(ValueTask.CompletedTask);
            configuration.WriteTo(sink.Object);
        }
        var logger = configuration.CreateLogger();

        await logger.DisposeAsync();

        foreach (var sink in sinks)
            sink.As<IAsyncDisposable>().Verify(disposable => disposable.DisposeAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateLogger_DisposeAsync_DisposesMixedSinksOnce()
    {
        var synchronous = new Mock<ILogEventSink>();
        synchronous.As<IDisposable>();
        var asynchronous = new Mock<ILogEventSink>();
        asynchronous.As<IDisposable>();
        asynchronous.As<IAsyncDisposable>().Setup(sink => sink.DisposeAsync()).Returns(ValueTask.CompletedTask);
        var logger = new SerilogConfiguration().WriteTo(synchronous.Object).WriteTo(asynchronous.Object).CreateLogger();

        await logger.DisposeAsync();

        synchronous.As<IDisposable>().Verify(sink => sink.Dispose(), Times.Once);
        asynchronous.As<IAsyncDisposable>().Verify(sink => sink.DisposeAsync(), Times.Once);
        asynchronous.As<IDisposable>().Verify(sink => sink.Dispose(), Times.Never);
    }
#endif

    [Fact]
    public void CreateLogger_Dispose_FlushesBatchingSink()
    {
        var emitted = new List<LogEvent>();
        var batching = new Mock<IBatchedLogEventSink>();
        batching.Setup(sink => sink.EmitBatchAsync(It.IsAny<IReadOnlyCollection<LogEvent>>()))
            .Callback<IReadOnlyCollection<LogEvent>>(events => emitted.AddRange(events))
            .Returns(Task.CompletedTask);
        var configuration = new SerilogConfiguration().WriteTo(new CollectingSink());
        configuration.Configure(logger => logger.WriteTo.Sink(batching.Object, new global::Serilog.Configuration.BatchingOptions
        {
            EagerlyEmitFirstEvent = false,
            BufferingTimeLimit = TimeSpan.FromHours(1)
        }));
        var logger = configuration.CreateLogger();
        logger.Error("buffered event");

        logger.Dispose();

        Assert.Equal("buffered event", Assert.Single(emitted).RenderMessage());
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
