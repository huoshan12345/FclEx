namespace FclEx.Serilog;

public class LogEventMutateSink : ILogEventSink
{
    protected readonly ILogEventSink _sink;
    protected readonly Action<LogEvent>? _mutator;

    public LogEventMutateSink(ILogEventSink sink, Action<LogEvent>? mutator)
    {
        _sink = sink;
        _mutator = mutator;
    }

    public virtual void Emit(LogEvent logEvent)
    {
        Mutate(logEvent);
        _sink.Emit(logEvent);
    }

    protected internal virtual void Mutate(LogEvent logEvent)
    {
        logEvent.HandleLogException();
        logEvent.HandleSimpleException();
        _mutator?.Invoke(logEvent);
    }
}