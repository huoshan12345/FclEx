using static FclEx.Serilog.Fields;

namespace FclEx.Serilog;

public class LogEventSinkAdapter : ILogEventSink
{
    protected readonly ILogEventSink _sink;
    protected readonly Action<LogEvent>? _modifier;

    public LogEventSinkAdapter(ILogEventSink sink, Action<LogEvent>? modifier)
    {
        _sink = sink;
        _modifier = modifier;
    }

    public virtual void Emit(LogEvent logEvent)
    {
        SetLevel(logEvent);
        _modifier?.Invoke(logEvent);
        _sink.Emit(logEvent);
    }

    protected internal virtual void SetLevel(LogEvent logEvent)
    {
        var level = logEvent.Level;

        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (logEvent.Exception is LogException logException)
        {
            level = logException.Level;
        }

        if (level != logEvent.Level)
        {
            LogEvent_Level.SetValue(logEvent, level);
        }
    }
}