namespace FclEx.Serilog.Filtering;

public class ExceptionExcluderTests
{
    [Theory]
    [InlineData(true, true, false, true, 1)]
    [InlineData(true, true, true, false, 1)]
    [InlineData(true, false, false, false, 0)]
    [InlineData(false, true, false, false, 0)]
    public void ShouldExclude_OnlyTestsCurrentExceptionAfterSourceMatches(bool sourceMatches, bool hasException, bool formatted, bool expected, int expectedCalls)
    {
        var calls = 0;
        Exception? exception = hasException ? new InvalidOperationException("failure") : null;
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Error, exception, "message");
        logEvent.AddOrUpdateProperty(new LogEventProperty(Constants.SourceContext, new ScalarValue(sourceMatches ? "App.Service" : "Other")));
        if (formatted)
            logEvent.FormatException();
        ExceptionExcluder rule = ("App", value =>
        {
            calls++;
            return value is InvalidOperationException;
        });

        Assert.Equal(expected, rule.ShouldExclude(logEvent));
        Assert.Equal(expectedCalls, calls);
    }
}
