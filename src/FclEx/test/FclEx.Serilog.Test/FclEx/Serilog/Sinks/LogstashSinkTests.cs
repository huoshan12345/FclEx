using FclEx.Serilog.Sinks.Logstash;

namespace FclEx.Serilog.Sinks;

public class LogstashSinkTests
{
    private readonly ITestOutputHelper _output;

    public LogstashSinkTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [LocalOnlyFact]
    public void Tcp_Test()
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Verbose()
            .WriteTo.TestOutput(_output)
            .CreateLogger();

        var logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Verbose()
            .WriteTo.Logstash("tcp://localhost:5050")
            .Enrich.With(new LogEnricher(nameof(LogstashSinkTests)))
            .CreateLogger()
            .ForContext<LogstashSinkTests>();

        for (var i = 0; i < 10; i++)
        {
            // logger.Information("test message: " + i + "\n");
            logger.Error(new SimpleException("Error"), "test message: " + i);
        }
        Thread.Sleep(TimeSpan.FromSeconds(10));
    }
}