namespace FclEx.Serilog.Extensions;

public partial class LoggerSinkConfigurationExtensionsTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Wrap_FallbackChain_ForwardsFailureNotifications(bool useFormattingWrapper)
    {
        var primary = new Mock<ILogEventSink>();
        ILoggingFailureListener? listener = null;
        primary.As<ISetLoggingFailureListener>()
            .Setup(sink => sink.SetFailureListener(It.IsAny<ILoggingFailureListener>()))
            .Callback<ILoggingFailureListener>(value => listener = value);
        var fallback = new CollectingSink();
        using var logger = new LoggerConfiguration().WriteTo.FallbackChain(
            first =>
            {
                if (useFormattingWrapper)
                    first.Wrap(inner => new LogEventMutateSink(inner, value => value.FormatException()), inner => inner.Sink(primary.Object));
                else
                    first.Sink(primary.Object);
            },
            second => second.Sink(fallback)).CreateLogger();
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Error, null, "recover this event");
        logger.Write(logEvent);

        // Batching sinks report failed batches through this listener after Emit returns.
        listener?.OnLoggingFailed(primary.Object, LoggingFailureKind.Permanent, "batch failed", new[] { logEvent }, null);

        Assert.Same(logEvent, Assert.Single(fallback.Events));
    }

    [Fact]
    public void Wrap_DisposableWrapper_DisposesInnerOnce()
    {
        var sink = new Mock<ILogEventSink>();
        sink.As<IDisposable>();
        var logger = new LoggerConfiguration().WriteTo.Wrap(
            inner => new LogEventMutateSink(inner, null),
            inner => inner.Sink(sink.Object)).CreateLogger();

        logger.Dispose();

        sink.As<IDisposable>().Verify(disposable => disposable.Dispose(), Times.Once);
    }

    [Fact(Explicit = true)]
    public async Task NewRelic_Test()
    {
        var logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.NewRelic(licenseKey: "")
            .CreateLogger();

        for (var i = 0; i < 10; i++)
        {
            logger.Information(i + "_" + Random.Shared.NextString(40));
        }

#if NET6_0_OR_GREATER
        await logger.DisposeAsync();
#else
        logger.Dispose();
#endif
        await Log.CloseAndFlushAsync();
    }
}
