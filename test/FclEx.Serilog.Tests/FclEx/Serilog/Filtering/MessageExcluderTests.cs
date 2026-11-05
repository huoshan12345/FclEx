namespace FclEx.Serilog.Filtering;

public class MessageExcluderTests
{
    [Theory]
    [InlineData("Needle", "contains Needle", "unused", true)]
    [InlineData("Needle", "contains {Value}", "Needle", false)]
    [InlineData("Needle", "contains {Needle}", "unrelated", true)]
    [InlineData("needle", "contains Needle", "unused", false)]
    [InlineData("", "contains anything", "unused", false)]
    public void ShouldExclude_SearchesRawTemplateInsteadOfRenderedValues(string needle, string template, string value, bool expected)
    {
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Error, null, template, value);
        MessageExcluder rule = (null, needle);

        Assert.Equal(expected, rule.ShouldExclude(logEvent));
    }

    [Theory]
    [InlineData(null, LogEventLevel.Error, true)]
    [InlineData("FclEx", LogEventLevel.Error, true)]
    [InlineData("Other", LogEventLevel.Error, false)]
    [InlineData(null, LogEventLevel.Warning, false)]
    public void ShouldExclude_AppliesSourceAndMaximum(string? source, LogEventLevel maximum, bool expected)
    {
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Error, null, "Needle");
        logEvent.AddOrUpdateProperty(new LogEventProperty(Constants.SourceContext, new ScalarValue("FclEx.Service")));
        MessageExcluder rule = (source, "Needle", maximum);

        Assert.Equal(expected, rule.ShouldExclude(logEvent));
    }
}
