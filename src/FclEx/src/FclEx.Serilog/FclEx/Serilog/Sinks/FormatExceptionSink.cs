using System.Reflection;

namespace FclEx.Serilog.Sinks;

public class FormatExceptionSink : ILogEventSink
{
    public static readonly FieldInfo LogEvent_Exception
        = typeof(LogEvent).GetRequiredField($"<{nameof(LogEvent.Exception)}>k__BackingField");

    protected readonly ILogEventSink _sink;

    public FormatExceptionSink(ILogEventSink sink)
    {
        _sink = sink;
    }

    public void Emit(LogEvent logEvent)
    {
        FormatException(logEvent);
        _sink.Emit(logEvent);
    }

    protected internal virtual void FormatException(LogEvent logEvent)
    {
        if (logEvent.Exception is null or FormattedException)
            return;

        var ex = new FormattedException(logEvent.Exception);
        LogEvent_Exception.SetValue(logEvent, ex);
    }
}