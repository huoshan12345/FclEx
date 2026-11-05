namespace FclEx.Serilog.Sinks;

public class FormatExceptionSinkTests
{
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
