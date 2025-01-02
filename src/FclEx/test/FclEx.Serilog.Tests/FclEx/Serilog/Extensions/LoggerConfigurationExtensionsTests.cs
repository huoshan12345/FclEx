namespace FclEx.Serilog.Extensions;

[Collection(nameof(Console))]
public class LoggerConfigurationExtensionsTests
{
    [Fact]
    public async Task WrapAllSinks_Test()
    {
        using var listener = new LogEventListener();
        await using var logger = new LoggerConfiguration()
            .WriteTo.Sink(listener)
            .WrapAllSinks(sink => new LogEventMutateSink(sink, null))
            .CreateLogger();

        logger.Information(new LogException("test", LogEventLevel.Warning), "");

        var flag = await listener.WaitAsync(1, TimeSpan.FromSeconds(1));
        Assert.True(flag);

        var logEvent = listener.Events.First();
        Assert.Equal(LogEventLevel.Warning, logEvent.Level);
    }
}