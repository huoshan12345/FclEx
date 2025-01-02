namespace FclEx.Serilog.Sinks;

public class LogEventMutateSinkTests(ITestOutputHelper output)
{
    private static readonly Mock<ILogEventSink> _sink = new();
    private static readonly LogEventMutateSink Adapter = new(_sink.Object, null);

    public static readonly IEnumerable<object[]> Levels = Enum.GetValues<LogEventLevel>()
        .Select(m => new object[] { m });

    [Theory]
    [MemberData(nameof(Levels))]
    public void SetLevel_LogException_Test(LogEventLevel level)
    {
        var ex = new LogException("", level);
        var logEvent = new LogEvent(DateTimeOffset.UtcNow, LogEventLevel.Error, ex,
            new MessageTemplate(ex.Message, []), []);

        Adapter.SetLevel(logEvent);

        Assert.Equal(level, logEvent.Level);
    }
}