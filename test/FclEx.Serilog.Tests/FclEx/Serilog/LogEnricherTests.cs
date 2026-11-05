namespace FclEx.Serilog;

public class LogEnricherTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("orders")]
    public void Enrich_AddsServiceAndHostMetadata(string? service)
    {
        var sink = new CollectingSink();
        using var logger = new LoggerConfiguration().Enrich.With(new LogEnricher(service)).WriteTo.Sink(sink).CreateLogger();

        logger.Information("message");

        var properties = Assert.Single(sink.Events).Properties;
        Assert.Equal(service, Assert.IsType<ScalarValue>(properties["Service"]).Value);
        Assert.Equal(LogEnricher.EntryAssembly, Assert.IsType<ScalarValue>(properties["EntryAssembly"]).Value);
        Assert.Equal(LogEnricher.OsType, Assert.IsType<ScalarValue>(properties["OsType"]).Value);
        Assert.Equal(LogEnricher.OsName, Assert.IsType<ScalarValue>(properties["OsName"]).Value);
        Assert.Equal(LogEnricher.HostName, Assert.IsType<ScalarValue>(properties["HostName"]).Value);
        Assert.Equal(LogEnricher.OsInfo, Assert.IsType<ScalarValue>(properties["OsInfo"]).Value);
    }

    [Fact]
    public void Enrich_PreservesExistingServiceAndHostProperties()
    {
        var sink = new CollectingSink();
        using var logger = new LoggerConfiguration().Enrich.With(new LogEnricher("default"))
            .WriteTo.Sink(sink).CreateLogger();

        logger.ForContext("Service", "event service").ForContext("HostName", "event host").Information("message");

        var properties = Assert.Single(sink.Events).Properties;
        Assert.Equal("event service", Assert.IsType<ScalarValue>(properties["Service"]).Value);
        Assert.Equal("event host", Assert.IsType<ScalarValue>(properties["HostName"]).Value);
        Assert.True(properties.ContainsKey("OsType"));
    }
}
