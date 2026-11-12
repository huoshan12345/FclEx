namespace FclEx.Serilog.Extensions.LoggerConfigurationExtensions;

public class GetSinksTests
{
    [Fact]
    public void GetSinks_ReturnsLiveListAndExcludesAuditSinks()
    {
        var configuration = new LoggerConfiguration();
        var sinks = configuration.GetSinks();
        Assert.Empty(sinks);
        Assert.Same(sinks, configuration.GetSinks());
        var normal = new CollectingSink();
        var audit = new CollectingSink();
        configuration.WriteTo.Sink(normal).AuditTo.Sink(audit);
        Assert.Same(normal, Assert.Single(sinks));

        var replacement = new CollectingSink();
        sinks.Clear();
        sinks.Add(replacement);
        using var logger = configuration.CreateLogger();
        logger.Information("message");
        Assert.Empty(normal.Events);
        Assert.Single(replacement.Events);
        Assert.Single(audit.Events);
    }

    [Fact]
    public void RemovingSink_DoesNotDisposeItOrReconfigureAnExistingLogger()
    {
        using var sink = new DisposableSink();
        var configuration = new LoggerConfiguration().WriteTo.Sink(sink);
        using (var logger = configuration.CreateLogger())
        {
            configuration.GetSinks().Clear();
            Assert.False(sink.Disposed);
            logger.Information("still registered in the logger");
            Assert.Single(sink.Events);
        }
        Assert.True(sink.Disposed);
    }

    private sealed class DisposableSink : ILogEventSink, IDisposable
    {
        public List<LogEvent> Events { get; } = [];
        public bool Disposed { get; private set; }
        public void Emit(LogEvent logEvent) => Events.Add(logEvent);
        public void Dispose() => Disposed = true;
    }
}
