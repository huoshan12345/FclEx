namespace FclEx.Serilog.Sinks;

public class LogEventMutateSinkTests
{
    [Fact]
    public void Emit_MutatorFailure_PropagatesWithoutForwardingEvent()
    {
        var failure = new InvalidOperationException("mutation failed");
        var sink = new Mock<ILogEventSink>();
        using var wrapper = new LogEventMutateSink(sink.Object, _ => throw failure);
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Error, null, "message");

        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => wrapper.Emit(logEvent)));
        sink.Verify(value => value.Emit(It.IsAny<LogEvent>()), Times.Never);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Dispose_RepeatedMixedCalls_ReleaseInnerSinkOnce(bool asyncFirst)
    {
        var sink = new Mock<ILogEventSink>();
        sink.As<IDisposable>();
        sink.As<IAsyncDisposable>().Setup(value => value.DisposeAsync()).Returns(ValueTask.CompletedTask);
        var wrapper = new LogEventMutateSink(sink.Object, null);

        if (asyncFirst)
            await wrapper.DisposeAsync();
        else
            wrapper.Dispose();
        wrapper.Dispose();
        await wrapper.DisposeAsync();

        sink.As<IDisposable>().Verify(value => value.Dispose(), asyncFirst ? Times.Never() : Times.Once());
        sink.As<IAsyncDisposable>().Verify(value => value.DisposeAsync(), asyncFirst ? Times.Once() : Times.Never());
    }

    [Fact]
    public void Dispose_DisposesWrappedSink()
    {
        var sink = new Mock<ILogEventSink>();
        var disposable = sink.As<IDisposable>();
        var adapter = new LogEventMutateSink(sink.Object, null);

        adapter.Dispose();

        disposable.Verify(m => m.Dispose(), Times.Once);
    }

    [Fact]
    public void Dispose_WithNonDisposableSink_Completes()
    {
        var adapter = new LogEventMutateSink(new CollectingSink(), null);

        adapter.Dispose();
    }

    [Fact]
    public async Task DisposeAsync_DisposesSynchronousOnlyWrappedSink()
    {
        var sink = new Mock<ILogEventSink>();
        var disposable = sink.As<IDisposable>();
        var adapter = new LogEventMutateSink(sink.Object, null);

        await adapter.DisposeAsync();

        disposable.Verify(m => m.Dispose(), Times.Once);
    }

    [Fact]
    public async Task DisposeAsync_WithNonDisposableSink_Completes()
    {
        var adapter = new LogEventMutateSink(new CollectingSink(), null);

        await adapter.DisposeAsync();
    }

    [Fact]
    public async Task DisposeAsync_WaitsForWrappedSink()
    {
        var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var sink = new Mock<ILogEventSink>();
        var disposable = sink.As<IAsyncDisposable>();
        disposable.Setup(m => m.DisposeAsync()).Returns(new ValueTask(completion.Task));
        var adapter = new LogEventMutateSink(sink.Object, null);

        var disposal = adapter.DisposeAsync().AsTask();

        Assert.False(disposal.IsCompleted);
        completion.SetResult(true);
        await disposal;
        disposable.Verify(m => m.DisposeAsync(), Times.Once);
    }

    [Fact]
    public async Task DisposeAsync_PropagatesWrappedSinkFailure()
    {
        var exception = new InvalidOperationException("disposal failed");
        var sink = new Mock<ILogEventSink>();
        var disposable = sink.As<IAsyncDisposable>();
        disposable.Setup(m => m.DisposeAsync()).Returns(new ValueTask(Task.FromException(exception)));
        var adapter = new LogEventMutateSink(sink.Object, null);

        var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => adapter.DisposeAsync().AsTask());

        Assert.Same(exception, actual);
    }

#if NET6_0_OR_GREATER
    [Fact]
    public async Task DisposeLoggerAsync_DisposesAsyncOnlyWrappedSink()
    {
        var sink = new Mock<ILogEventSink>();
        var disposable = sink.As<IAsyncDisposable>();
        disposable.Setup(m => m.DisposeAsync()).Returns(ValueTask.CompletedTask);
        var logger = new LoggerConfiguration()
            .WriteTo.Sink(sink.Object)
            .WrapAllSinks(inner => new LogEventMutateSink(inner, null))
            .CreateLogger();

        await logger.DisposeAsync();

        disposable.Verify(m => m.DisposeAsync(), Times.Once);
    }

    [Fact]
    public async Task DisposeLoggerAsync_PrefersAsyncDisposalForWrappedSink()
    {
        var sink = new Mock<ILogEventSink>();
        var synchronous = sink.As<IDisposable>();
        var asynchronous = sink.As<IAsyncDisposable>();
        asynchronous.Setup(m => m.DisposeAsync()).Returns(ValueTask.CompletedTask);
        var logger = new LoggerConfiguration()
            .WriteTo.Sink(sink.Object)
            .WrapAllSinks(inner => new LogEventMutateSink(inner, null))
            .CreateLogger();

        await logger.DisposeAsync();

        asynchronous.Verify(m => m.DisposeAsync(), Times.Once);
        synchronous.Verify(m => m.Dispose(), Times.Never);
    }
#endif

    private static readonly Mock<ILogEventSink> _sink = new();
    private static readonly LogEventMutateSink Adapter = new(_sink.Object, null);

    public static readonly TheoryData<LogEventLevel> Levels = Enum.GetValues<LogEventLevel>().ToTheoryData();

    [Theory]
    [MemberData(nameof(Levels))]
    public void SetLevel_LogException_Test(LogEventLevel level)
    {
        var ex = new LogException("", level.ToExtensionsLevel());
        var logEvent = new LogEvent(DateTimeOffset.UtcNow, LogEventLevel.Error, ex,
            new MessageTemplate(ex.Message, []), []);

        Adapter.Mutate(logEvent);

        Assert.Equal(level, logEvent.Level);
    }
}
