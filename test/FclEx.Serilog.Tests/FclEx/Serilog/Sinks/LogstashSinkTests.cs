namespace FclEx.Serilog.Sinks;

public class LogstashSinkTests(ITestOutputHelper output)
{
    [LocalOnlyFact]
    public async Task Tcp_Test()
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Verbose()
            .WriteTo.TestOutput(output)
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
#if NET5_0_OR_GREATER
        await Log.CloseAndFlushAsync();   
#else
        Log.CloseAndFlush();
#endif
        await Task.Delay(TimeSpan.FromSeconds(1));
    }
}