using System.Threading;

namespace FclEx.Serilog.Extensions;

public partial class LoggerSinkConfigurationExtensionsTests
{
    [Theory]
    [InlineData(1, false)]
    [InlineData(2, false)]
    [InlineData(1, true)]
    [InlineData(2, true)]
    public void Wrap_GroupFallback_ReceivesEachSinkFailure(int sinkCount, bool restricted)
    {
        var listeners = new ILoggingFailureListener?[sinkCount];
        var sinks = Enumerable.Range(0, sinkCount).Select(index =>
        {
            var sink = new Mock<ILogEventSink>();
            sink.As<ISetLoggingFailureListener>().Setup(value => value.SetFailureListener(It.IsAny<ILoggingFailureListener>()))
                .Callback<ILoggingFailureListener>(listener => listeners[index] = listener);
            return sink;
        }).ToArray();
        var fallback = new CollectingSink();
        using var logger = new LoggerConfiguration().WriteTo.FallbackChain(
            first => first.Wrap(inner => new LogEventMutateSink(inner, value => value.FormatException()), inner =>
            {
                foreach (var sink in sinks)
                    inner.Sink(sink.Object, restrictedToMinimumLevel: restricted ? LogEventLevel.Warning : LogEventLevel.Verbose);
            }), second => second.Sink(fallback)).CreateLogger();
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Error, null, "recover this event");

        foreach (var listener in listeners)
        {
            Assert.NotNull(listener);
            listener.OnLoggingFailed(this, LoggingFailureKind.Permanent, "batch failed", new[] { logEvent }, null);
        }

        Assert.Equal(sinkCount, fallback.Events.Count);
        Assert.All(fallback.Events, value => Assert.Same(logEvent, value));
    }

    [Fact]
    public void Wrap_OwningWrapper_PreservesFailureNotificationsAndDisposal()
    {
        var sink = new Mock<ILogEventSink>();
        sink.As<IDisposable>();
        var listener = new Mock<ILoggingFailureListener>();
        var forwarding = sink.As<ISetLoggingFailureListener>();
        var logger = new LoggerConfiguration().WriteTo.Fallible(
            first => first.Wrap(inner =>
            {
                Assert.IsAssignableFrom<IDisposable>(inner);
                Assert.IsAssignableFrom<IAsyncDisposable>(inner);
                Assert.IsAssignableFrom<ISetLoggingFailureListener>(inner);
                return new LogEventMutateSink(inner, null);
            }, group => group.Sink(sink.Object)), listener.Object).CreateLogger();

        logger.Dispose();

        sink.As<IDisposable>().Verify(value => value.Dispose(), Times.Once);
        forwarding.Verify(value => value.SetFailureListener(listener.Object), Times.Once);
    }

    [Fact]
    public void Wrap_DisposesWrapperBeforeInnerSinkOnce()
    {
        var order = new List<string>();
        var sink = new Mock<ILogEventSink>();
        sink.As<IDisposable>().Setup(value => value.Dispose()).Callback(() => order.Add("inner"));
        var logger = new LoggerConfiguration().WriteTo.Wrap(
            inner => new ResourceOwningWrapper(inner, order), group => group.Sink(sink.Object)).CreateLogger();

        logger.Dispose();
        logger.Dispose();

        Assert.Equal(new[] { "wrapper", "inner" }, order);
    }

#if NET6_0_OR_GREATER
    [Fact]
    public async Task Wrap_DisposeAsync_AwaitsWrapperBeforeDisposingInnerSinksOnce()
    {
        var order = new List<string>();
        var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var sink = new Mock<ILogEventSink>();
        sink.As<IDisposable>();
        sink.As<IAsyncDisposable>().Setup(value => value.DisposeAsync())
            .Callback(() => order.Add("inner async")).Returns(ValueTask.CompletedTask);
        var logger = new LoggerConfiguration().WriteTo.Wrap(
            inner => new ResourceOwningWrapper(inner, order, completion.Task), group => group.Sink(sink.Object)).CreateLogger();

        var disposal = logger.DisposeAsync().AsTask();
        Assert.False(disposal.IsCompleted);
        Assert.Empty(order);
        completion.SetResult(true);
        await disposal;

        Assert.Equal(new[] { "wrapper async", "inner async" }, order);
        sink.As<IDisposable>().Verify(value => value.Dispose(), Times.Never);
        sink.As<IAsyncDisposable>().Verify(value => value.DisposeAsync(), Times.Once);
    }
#endif

    [Fact]
    public void Wrap_InnerSinkDisposalFails_StillDisposesOtherSinks()
    {
        var failure = new IOException("inner cleanup failed");
        var first = new Mock<ILogEventSink>();
        first.As<IDisposable>().Setup(value => value.Dispose()).Throws(failure);
        var second = new Mock<ILogEventSink>();
        second.As<IDisposable>();
        var logger = new LoggerConfiguration().WriteTo.Wrap(inner => new LogEventMutateSink(inner, null),
            group => group.Sink(first.Object).WriteTo.Sink(second.Object)).CreateLogger();

        var actual = Assert.Throws<IOException>(() => logger.Dispose());

        Assert.Same(failure, actual);
        first.As<IDisposable>().Verify(value => value.Dispose(), Times.Once);
        second.As<IDisposable>().Verify(value => value.Dispose(), Times.Once);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Wrap_ConstructionFails_DisposesRegisteredAsyncSink(bool wrapperFactoryFails)
    {
        var failure = new InvalidOperationException("construction failed");
        var sink = new Mock<ILogEventSink>();
        sink.As<IAsyncDisposable>().Setup(value => value.DisposeAsync()).Returns(ValueTask.CompletedTask);
        var configuration = new LoggerConfiguration();

        var actual = Assert.Throws<InvalidOperationException>(() => configuration.WriteTo.Wrap(
            _ => throw failure,
            group =>
            {
                group.Sink(sink.Object);
                if (!wrapperFactoryFails)
                    throw failure;
            }));

        Assert.Same(failure, actual);
        sink.As<IAsyncDisposable>().Verify(value => value.DisposeAsync(), Times.Once);
    }

    private sealed class ResourceOwningWrapper(ILogEventSink inner, List<string> order, Task? completion = null)
        : ILogEventSink, IDisposable, IAsyncDisposable
    {
        private int _disposed;

        public void Emit(LogEvent logEvent) => inner.Emit(logEvent);

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;
            order.Add("wrapper");
            (inner as IDisposable)?.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;
            if (completion != null)
                await completion;
            order.Add("wrapper async");
            if (inner is IAsyncDisposable asynchronous)
                await asynchronous.DisposeAsync();
            else
                (inner as IDisposable)?.Dispose();
        }
    }
}
