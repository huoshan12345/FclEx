namespace FclEx.Serilog.Filtering;

public class SourceExcluderTests
{
    [Theory]
    [InlineData("App", LogEventLevel.Information, null, true)]
    [InlineData("App.Service", LogEventLevel.Warning, LogEventLevel.Warning, true)]
    [InlineData("App.Service", LogEventLevel.Error, LogEventLevel.Warning, false)]
    [InlineData("AppOther", LogEventLevel.Information, null, false)]
    [InlineData("app.Service", LogEventLevel.Information, null, false)]
    [InlineData(null, LogEventLevel.Information, null, false)]
    public void ShouldExclude_RequiresHierarchicalSourceAndInclusiveMaximum(string? source, LogEventLevel level, LogEventLevel? maximum, bool expected)
    {
        var logEvent = SerilogHelper.CreateLogEvent(level, null, "message");
        logEvent.RemovePropertyIfPresent(Constants.SourceContext);
        if (source != null)
            logEvent.AddOrUpdateProperty(new LogEventProperty(Constants.SourceContext, new ScalarValue(source)));
        SourceExcluder rule = ("App", maximum);

        Assert.Equal(expected, rule.ShouldExclude(logEvent));
    }
}
