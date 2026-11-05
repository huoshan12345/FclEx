namespace FclEx.Serilog.Filtering;

public class PropertyExcluderTests
{
    [Theory]
    [InlineData(0, null, false)]
    [InlineData(1, null, true)]
    [InlineData(2, null, true)]
    [InlineData(3, null, true)]
    [InlineData(4, null, true)]
    [InlineData(1, "Needle", true)]
    [InlineData(1, "needle", false)]
    [InlineData(1, "", true)]
    [InlineData(2, "5", false)]
    [InlineData(3, "", false)]
    [InlineData(4, "Needle", false)]
    public void ShouldExclude_DistinguishesPresenceFromStringSubstring(int valueKind, string? substring, bool expected)
    {
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Error, null, "message");
        if (valueKind != 0)
        {
            LogEventPropertyValue value = valueKind switch
            {
                1 => new ScalarValue("contains Needle"),
                2 => new ScalarValue(5),
                3 => new ScalarValue(null),
                _ => new StructureValue(new[] { new LogEventProperty("Child", new ScalarValue("Needle")) })
            };
            logEvent.AddOrUpdateProperty(new LogEventProperty("Payload", value));
        }
        PropertyExcluder rule = (null, "Payload", substring);

        Assert.Equal(expected, rule.ShouldExclude(logEvent));
        Assert.False(new PropertyExcluder(null, "payload", substring).ShouldExclude(logEvent));
    }

    [Theory]
    [InlineData("FclEx", LogEventLevel.Error, true)]
    [InlineData("Other", LogEventLevel.Error, false)]
    [InlineData("FclEx", LogEventLevel.Warning, false)]
    public void ShouldExclude_CombinesSourcePropertyAndMaximum(string source, LogEventLevel maximum, bool expected)
    {
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Error, null, "{Payload}", "Needle");
        logEvent.AddOrUpdateProperty(new LogEventProperty(Constants.SourceContext, new ScalarValue("FclEx.Service")));
        PropertyExcluder rule = (source, "Payload", "Needle", maximum);

        Assert.Equal(expected, rule.ShouldExclude(logEvent));
    }
}
