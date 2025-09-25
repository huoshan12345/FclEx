namespace FclEx.Serilog.Sinks;

public class LogEventMutateSinkTests
{
    private static readonly Mock<ILogEventSink> _sink = new();
    private static readonly LogEventMutateSink Adapter = new(_sink.Object, null);

    public static readonly IEnumerable<object[]> Levels = EnumHelper.GetValues<LogEventLevel>()
        .Select(m => new object[] { m });

    [Theory]
    [MemberData(nameof(Levels))]
    public void SetLevel_LogException_Test(LogEventLevel level)
    {
        var ex = new LogException("", level.ToExtensionsLevel());
        var logEvent = new LogEvent(DateTimeOffset.UtcNow, LogEventLevel.Error, ex,
            new MessageTemplate(ex.Message, []), []);

        Adapter.Mutate(logEvent);

        Assert.Equal(level, logEvent.Level);
    }
}