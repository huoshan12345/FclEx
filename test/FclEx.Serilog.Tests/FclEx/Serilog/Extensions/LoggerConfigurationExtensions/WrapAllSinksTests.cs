using Microsoft.Extensions.Logging;

namespace FclEx.Serilog.Extensions.LoggerConfigurationExtensions;

[Collection(nameof(Console))]
public class WrapAllSinksTests
{
    [Fact]
    public async Task WrapAllSinks_Test()
    {
        var sink = new CollectingSink();
        await using var logger = new LoggerConfiguration()
            .WriteTo.Sink(sink)
            .WrapAllSinks(m => new LogEventMutateSink(m, null))
            .CreateLogger();

        logger.Information(new LogException("test", LogLevel.Warning), "");

        var logEvent = Assert.Single(sink.Events);
        Assert.Equal(LogEventLevel.Warning, logEvent.Level);
    }

}