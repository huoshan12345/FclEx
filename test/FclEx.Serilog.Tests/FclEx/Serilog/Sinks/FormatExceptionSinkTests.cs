namespace FclEx.Serilog.Sinks;

public class FormatExceptionSinkTests
{
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
