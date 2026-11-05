using Microsoft.Extensions.Logging;

namespace FclEx.Serilog.Extensions.LoggerConfigurationExtensions;

public class WrapAllSinksTests
{
    [Fact]
    public void DisposeLogger_FlushesWrappedBatchingSink()
    {
        var emittedEvents = new List<LogEvent>();
        var sink = new Mock<IBatchedLogEventSink>();
        sink.Setup(m => m.EmitBatchAsync(It.IsAny<IReadOnlyCollection<LogEvent>>()))
            .Callback<IReadOnlyCollection<LogEvent>>(events => emittedEvents.AddRange(events))
            .Returns(Task.CompletedTask);
        var logger = new LoggerConfiguration()
            .WriteTo.Sink(sink.Object, new global::Serilog.Configuration.BatchingOptions
            {
                EagerlyEmitFirstEvent = false,
                BufferingTimeLimit = TimeSpan.FromHours(1)
            })
            .WrapAllSinks(inner => new LogEventMutateSink(inner, null))
            .CreateLogger();
        logger.Information("buffered event");

        logger.Dispose();

        sink.Verify(m => m.EmitBatchAsync(It.IsAny<IReadOnlyCollection<LogEvent>>()), Times.Once);
        Assert.Equal("buffered event", Assert.Single(emittedEvents).RenderMessage());
    }

    [Fact]
    public async Task WrapAllSinks_Test()
    {
        var sink = new CollectingSink();
#if NET6_0_OR_GREATER
        await
#endif
        using var logger = new LoggerConfiguration()
            .WriteTo.Sink(sink)
            .WrapAllSinks(m => new LogEventMutateSink(m, null))
            .CreateLogger();

        logger.Information(new LogException("test", LogLevel.Warning), "");

        var logEvent = Assert.Single(sink.Events);
        Assert.Equal(LogEventLevel.Warning, logEvent.Level);
    }

}
