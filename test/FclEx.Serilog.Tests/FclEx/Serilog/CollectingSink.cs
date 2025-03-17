namespace FclEx.Serilog;

public class CollectingSink : ILogEventSink
{
    public List<LogEvent> Events { get; } = [];

    public void Emit(LogEvent logEvent)
    {
        Events.Add(logEvent);
    }
}
