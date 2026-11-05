namespace FclEx.Serilog;

/// <summary>Wraps event exceptions for consistent formatting before forwarding events to an owned sink.</summary>
/// <param name="sink">The sink that receives formatted events and is disposed with this wrapper.</param>
/// <remarks>
/// Registering this wrapper directly with WriteTo.Sink transfers its disposal to the logger.
/// Use asynchronous disposal when the inner sink requires asynchronous cleanup.
/// </remarks>
/// <exception cref="ArgumentNullException"><paramref name="sink"/> is null.</exception>
public class FormatExceptionSink(ILogEventSink sink) : ILogEventSink, IDisposable, IAsyncDisposable
{
    protected readonly ILogEventSink _sink = sink ?? throw new ArgumentNullException(nameof(sink));

    public void Emit(LogEvent logEvent)
    {
        FormatException(logEvent);
        _sink.Emit(logEvent);
    }

    protected internal virtual void FormatException(LogEvent logEvent)
    {
        logEvent.FormatException();
    }

    /// <summary>Disposes the inner sink when it implements <see cref="IDisposable"/>.</summary>
    /// <remarks>Exceptions from inner sink disposal propagate to the caller.</remarks>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        (_sink as IDisposable)?.Dispose();
    }

    /// <summary>Awaits asynchronous inner sink disposal, falling back to synchronous disposal when necessary.</summary>
    /// <returns>An operation that completes when the inner sink has finished disposing.</returns>
    /// <remarks>
    /// Prefers <see cref="IAsyncDisposable"/> over <see cref="IDisposable"/> and does nothing if neither is implemented.
    /// Exceptions from inner sink disposal propagate to the caller.
    /// </remarks>
    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        if (_sink is IAsyncDisposable asynchronous)
            await asynchronous.DisposeAsync().ConfigureAwait(false);
        else
            (_sink as IDisposable)?.Dispose();
    }
}
