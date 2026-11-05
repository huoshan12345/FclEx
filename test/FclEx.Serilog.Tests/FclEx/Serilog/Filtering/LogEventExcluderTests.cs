namespace FclEx.Serilog.Filtering;

public class LogEventExcluderTests
{
    [Fact]
    public void ShouldExclude_PredicateFailure_Propagates()
    {
        var failure = new InvalidOperationException("predicate failed");
        var rule = new LogEventExcluder(_ => throw failure);
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Error, null, "message");

        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => rule.ShouldExclude(logEvent)));
    }

    [Fact]
    public void ShouldExclude_SeesOriginalEventAndReturnsPredicateResult()
    {
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Warning, null, "message");
        var rule = new LogEventExcluder(value => ReferenceEquals(logEvent, value) && value.Level == LogEventLevel.Warning);

        Assert.True(rule.ShouldExclude(logEvent));
    }
}
