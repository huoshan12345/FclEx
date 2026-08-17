namespace FclEx.Serilog.Sinks;

public class FormatExceptionSinkTests
{
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