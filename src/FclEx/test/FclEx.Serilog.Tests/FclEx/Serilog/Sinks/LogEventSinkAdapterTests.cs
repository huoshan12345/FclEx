namespace FclEx.Serilog.Sinks;

public class LogEventSinkAdapterTests
{
    private static readonly Mock<ILogEventSink> _sink = new();
    private static readonly LogEventSinkAdapter Adapter = new(_sink.Object, null);

    private readonly ITestOutputHelper _output;

    public LogEventSinkAdapterTests(ITestOutputHelper output)
    {
        _output = output;
    }

    public static readonly IEnumerable<object[]> Levels = Enum.GetValues<LogEventLevel>()
        .Select(m => new object[] { m });

    [Theory]
    [MemberData(nameof(Levels))]
    public void SetLevel_LogException_Test(LogEventLevel level)
    {
        var ex = new LogException("", level);
        var logEvent = new LogEvent(DateTimeOffset.UtcNow, LogEventLevel.Error, ex,
            new MessageTemplate(ex.Message, Enumerable.Empty<MessageTemplateToken>()), Enumerable.Empty<LogEventProperty>());

        Adapter.SetLevel(logEvent);

        Assert.Equal(level, logEvent.Level);
    }
}