namespace FclEx.Serilog;

public class FormatExceptionSink(ILogEventSink sink) : ILogEventSink
{
    protected readonly ILogEventSink _sink = sink;

    public void Emit(LogEvent logEvent)
    {
        FormatException(logEvent);
        _sink.Emit(logEvent);
    }

    protected internal virtual void FormatException(LogEvent logEvent)
    {
        logEvent.FormatException();
    }
}