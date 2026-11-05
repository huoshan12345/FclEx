using Serilog.Sinks.XUnit3;

namespace FclEx.Serilog.Sinks;

public class LogstashSinkTests
{
    [Theory]
    [InlineData("https://localhost:5050")]
    [InlineData("tpc://localhost:5050")]
    public void Constructor_UnsupportedScheme_Throws(string uri)
    {
        Assert.ThrowsAny<ArgumentException>(() => new LogstashSink(new LogstashSinkOptions(uri)));
    }

    [Fact(Explicit = true)]
    public async Task EmitBatchAsync_TailscaleTcp_Completes()
    {
        var sink = new LogstashSink(new LogstashSinkOptions("tcp://yecao-hk-4c8g-200m:5050"));
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Information, null,
            "FclEx.Serilog review test {TestId}", Guid.NewGuid());

        await sink.EmitBatchAsync(new[] { logEvent }).WaitAsync(TimeSpan.FromSeconds(20));
    }

    [Fact(Explicit = true)]
    public async Task Tcp_Test()
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Verbose()
            .WriteTo.XUnit3TestOutput()
            .CreateLogger();

        var logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Verbose()
            .WriteTo.Logstash("tcp://localhost:5050")
            .CreateLogger()
            .ForContext<LogstashSinkTests>();

        for (var i = 0; i < 10; i++)
        {
            // logger.Information("test message: " + i + "\n");
            logger.Error(new SimpleException("Error"), "test message: " + i);
        }

        await Log.CloseAndFlushAsync();
        await Task.Delay(TimeSpan.FromSeconds(1));
    }
}
