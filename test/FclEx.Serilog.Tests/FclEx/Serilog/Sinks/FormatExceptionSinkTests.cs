namespace FclEx.Serilog.Sinks;

public class FormatExceptionSinkTests
{
    [Fact]
    public void FormatException_DoesNotMutateSiblingSinkException()
    {
        var exception = new InvalidOperationException("failure");
        Exception? formatted = null;
        Exception? sibling = null;
        var first = new Mock<ILogEventSink>();
        first.Setup(sink => sink.Emit(It.IsAny<LogEvent>())).Callback<LogEvent>(logEvent => formatted = logEvent.Exception);
        var second = new Mock<ILogEventSink>();
        second.Setup(sink => sink.Emit(It.IsAny<LogEvent>())).Callback<LogEvent>(logEvent => sibling = logEvent.Exception);
        using var logger = new LoggerConfiguration().WriteTo.FormatException(inner => inner.Sink(first.Object))
            .WriteTo.Sink(second.Object).CreateLogger();

        logger.Error(exception, "message");

        Assert.IsType<FormattedException>(formatted);
        Assert.Same(exception, sibling);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void DisposeLogger_ExtensionRegistration_DisposesAllInnerSinksOnce(int sinkCount)
    {
        var sinks = Enumerable.Range(0, sinkCount).Select(_ => new Mock<ILogEventSink>()).ToArray();
        foreach (var sink in sinks)
            sink.As<IDisposable>();
        var logger = new LoggerConfiguration().WriteTo.FormatException(writeTo =>
        {
            foreach (var sink in sinks)
                writeTo.Sink(sink.Object);
        }).CreateLogger();

        logger.Dispose();

        foreach (var sink in sinks)
            sink.As<IDisposable>().Verify(disposable => disposable.Dispose(), Times.Once);
    }

    [Fact]
    public void Constructor_NullSink_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new FormatExceptionSink(null!));
    }

    [Fact]
    public async Task DisposeAsync_SynchronousOnlySink_DisposesInnerSink()
    {
        var sink = new Mock<ILogEventSink>();
        sink.As<IDisposable>();
        var wrapper = new FormatExceptionSink(sink.Object);

        await wrapper.DisposeAsync();

        sink.As<IDisposable>().Verify(disposable => disposable.Dispose(), Times.Once);
    }

    [Fact]
    public async Task Dispose_NonDisposableSink_Completes()
    {
        new FormatExceptionSink(new CollectingSink()).Dispose();
        await new FormatExceptionSink(new CollectingSink()).DisposeAsync();
    }

    [Fact]
    public async Task DisposeAsync_WaitsForInnerSink()
    {
        var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var sink = new Mock<ILogEventSink>();
        sink.As<IAsyncDisposable>().Setup(disposable => disposable.DisposeAsync()).Returns(new ValueTask(completion.Task));
        var wrapper = new FormatExceptionSink(sink.Object);

        var disposal = wrapper.DisposeAsync().AsTask();

        Assert.False(disposal.IsCompleted);
        completion.SetResult(true);
        await disposal;
        sink.As<IAsyncDisposable>().Verify(disposable => disposable.DisposeAsync(), Times.Once);
    }

    [Fact]
    public async Task DisposeAsync_InnerSinkFailure_Propagates()
    {
        var failure = new IOException("cleanup failed");
        var sink = new Mock<ILogEventSink>();
        sink.As<IAsyncDisposable>().Setup(disposable => disposable.DisposeAsync())
            .Returns(new ValueTask(Task.FromException(failure)));
        var wrapper = new FormatExceptionSink(sink.Object);

        var actual = await Assert.ThrowsAsync<IOException>(() => wrapper.DisposeAsync().AsTask());

        Assert.Same(failure, actual);
    }

    [Fact]
    public void DisposeLogger_DirectWrapperRegistration_DisposesInnerSink()
    {
        var sink = new Mock<ILogEventSink>();
        var disposable = sink.As<IDisposable>();
        var logger = new LoggerConfiguration().WriteTo.Sink(new FormatExceptionSink(sink.Object)).CreateLogger();

        logger.Dispose();

        disposable.Verify(m => m.Dispose(), Times.Once);
    }

#if NET6_0_OR_GREATER
    [Theory]
    [InlineData(true, 1)]
    [InlineData(true, 2)]
    [InlineData(false, 1)]
    public async Task DisposeLoggerAsync_PrefersAsyncDisposal(bool useExtension, int sinkCount)
    {
        var sinks = Enumerable.Range(0, sinkCount).Select(_ => new Mock<ILogEventSink>()).ToArray();
        foreach (var sink in sinks)
        {
            sink.As<IDisposable>();
            sink.As<IAsyncDisposable>().Setup(disposable => disposable.DisposeAsync()).Returns(ValueTask.CompletedTask);
        }
        var configuration = new LoggerConfiguration();
        if (useExtension)
            configuration.WriteTo.FormatException(writeTo =>
            {
                foreach (var sink in sinks)
                    writeTo.Sink(sink.Object);
            });
        else
            configuration.WriteTo.Sink(new FormatExceptionSink(sinks[0].Object));
        var logger = configuration.CreateLogger();

        await logger.DisposeAsync();

        foreach (var sink in sinks)
        {
            sink.As<IAsyncDisposable>().Verify(disposable => disposable.DisposeAsync(), Times.Once);
            sink.As<IDisposable>().Verify(disposable => disposable.Dispose(), Times.Never);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DisposeLoggerAsync_DisposesAsyncOnlyInnerSink(bool useExtension)
    {
        var sink = new Mock<ILogEventSink>();
        var asynchronous = sink.As<IAsyncDisposable>();
        asynchronous.Setup(m => m.DisposeAsync()).Returns(ValueTask.CompletedTask);
        var configuration = new LoggerConfiguration();
        if (useExtension)
            configuration.WriteTo.FormatException(writeTo => writeTo.Sink(sink.Object));
        else
            configuration.WriteTo.Sink(new FormatExceptionSink(sink.Object));
        var logger = configuration.CreateLogger();

        await logger.DisposeAsync();

        asynchronous.Verify(m => m.DisposeAsync(), Times.Once);
    }
#endif

    [Fact]
    public void DisposeLogger_DisposesWrappedSink()
    {
        var sink = new Mock<ILogEventSink>();
        var disposable = sink.As<IDisposable>();
        var logger = new LoggerConfiguration().WriteTo.FormatException(m => m.Sink(sink.Object)).CreateLogger();

        logger.Dispose();

        disposable.Verify(m => m.Dispose(), Times.Once);
    }

    [Fact]
    public void FormatException_Test()
    {
        var sink = new CollectingSink();
        var logger = new LoggerConfiguration().WriteTo
            .FormatException(m => m.Sink(sink))
            .CreateLogger();

        try
        {
            throw new InvalidOperationException();
        }
        catch (Exception ex)
        {
            logger.Error(ex, ex.Message);
        }

        Assert.Single(sink.Events);

        var logEvent = sink.Events[0];
        Assert.IsType<FormattedException>(logEvent.Exception);
    }
}
