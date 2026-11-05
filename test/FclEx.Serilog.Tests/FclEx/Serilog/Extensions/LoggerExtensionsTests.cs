namespace FclEx.Serilog.Extensions;

public class LoggerExtensionsTests
{
    [Fact]
    public void ActionError_AttachesExceptionAndStructuredErrorProperties()
    {
        var sink = new CollectingSink();
        using var logger = new LoggerConfiguration().WriteTo.Sink(sink).CreateLogger();
        var exception = new InvalidOperationException("storage unavailable");

        logger.ActionError("save order", exception);

        var logEvent = Assert.Single(sink.Events);
        Assert.Equal(LogEventLevel.Error, logEvent.Level);
        Assert.Same(exception, logEvent.Exception);
        Assert.Equal("save order", logEvent.GetPropertyValue<string>("Title"));
        Assert.Equal(exception.Message, logEvent.GetPropertyValue<string>("Error"));
    }

    [Fact]
    public void ForContext_StringSource_UsesCategoryLevelOverride()
    {
        var sink = new CollectingSink();
        using var logger = new LoggerConfiguration().MinimumLevel.Verbose().MinimumLevel.Override("App", LogEventLevel.Error)
            .WriteTo.Sink(sink).CreateLogger();
        var category = logger.ForContext("App.Service");

        category.Information("filtered");
        category.Error("recorded");

        Assert.Equal("App.Service", Assert.Single(sink.Events).GetSourceContext());
    }
}
