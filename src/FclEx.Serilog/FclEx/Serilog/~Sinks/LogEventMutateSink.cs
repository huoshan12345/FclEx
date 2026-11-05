namespace FclEx.Serilog;

public class LogEventMutateSink : ILogEventSink, IDisposable, IAsyncDisposable
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

    /// <summary>Disposes the wrapped sink when it implements <see cref="IDisposable"/>.</summary>
    /// <remarks>Exceptions raised by the wrapped sink during disposal propagate to the caller.</remarks>
    public void Dispose()
    {
        GC.SuppressFinalize(this);

        (_sink as IDisposable)?.Dispose();
    }

    /// <summary>Disposes the wrapped sink asynchronously when it implements <see cref="IAsyncDisposable"/>.</summary>
    /// <returns>An operation that completes when the wrapped sink has finished disposing.</returns>
    /// <remarks>
    /// Falls back to synchronous disposal when the wrapped sink only implements <see cref="IDisposable"/>.
    /// Completes without disposing a sink that implements neither interface. Exceptions raised during
    /// synchronous or asynchronous disposal propagate to the caller.
    /// </remarks>
    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        if (_sink is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
        else
        {
            (_sink as IDisposable)?.Dispose();
        }
    }
}
