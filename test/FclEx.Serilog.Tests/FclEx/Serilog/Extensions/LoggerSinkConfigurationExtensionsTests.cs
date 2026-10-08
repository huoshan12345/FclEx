namespace FclEx.Serilog.Extensions;

public class LoggerSinkConfigurationExtensionsTests
{
    [Fact(Explicit = true)]
    public async Task NewRelic_Test()
    {
        var logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.NewRelic(licenseKey: "")
            .CreateLogger();

        for (var i = 0; i < 10; i++)
        {
            logger.Information(i + "_" + Random.Shared.NextString(40));
        }

#if NET6_0_OR_GREATER
        await logger.DisposeAsync();
#else
        logger.Dispose();
#endif
        await Log.CloseAndFlushAsync();
    }
}