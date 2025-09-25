namespace FclEx.Serilog.Extensions;

public class LoggerSinkConfigurationExtensionsTests
{
    [LocalOnlyFact(Skip = "No license key")]
    public void NewRelic_Test()
    {
        var logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.NewRelic(licenseKey: "")
            .CreateLogger();

        for (var i = 0; i < 10; i++)
        {
            logger.Information(i + "_" + nameof(NewRelic_Test));
        }
    }
}