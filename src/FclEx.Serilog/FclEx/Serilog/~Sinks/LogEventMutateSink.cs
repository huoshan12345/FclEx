using System.Threading;

namespace FclEx.Serilog;

/// <summary>Handles FclEx exception levels and duplicate messages, then applies an optional mutator before forwarding an event.</summary>
/// <remarks>
/// Use WrapSinkPipeline to run this processing once before dispatching to all configured WriteTo sinks.
/// This sink does not reapply global filters after changing an event level. SerilogConfiguration adds
/// the earlier exception-level filter and minimum-level recheck when creating its logger.
/// </remarks>
public class LogEventMutateSink : ILogEventSink, IDisposable, IAsyncDisposable, ISetLoggingFailureListener
{
    /// <summary>The inner sink owned by this wrapper.</summary>
    protected readonly ILogEventSink _sink;
    /// <summary>Optional processing executed after built-in exception normalization.</summary>
    protected readonly Action<LogEvent>? _mutator;
    private int _disposed;

    /// <summary>Creates an exception-normalizing wrapper that owns the supplied sink.</summary>
    /// <param name="sink">The destination sink, released with this wrapper.</param>
    /// <param name="mutator">Additional processing after FclEx exception handling, or null to perform exception handling alone.</param>
    /// <exception cref="ArgumentNullException"><paramref name="sink"/> is null.</exception>
    public LogEventMutateSink(ILogEventSink sink, Action<LogEvent>? mutator)
    {
        _sink = Check.NotNull(sink);
        _mutator = mutator;
    }

    /// <summary>Normalizes the event in place, applies the custom mutator, and forwards it to the inner sink.</summary>
    /// <param name="logEvent">The event to process and forward.</param>
    /// <remarks>Normalization, mutator, and inner-sink errors propagate; the inner sink is not called if mutation fails.</remarks>
    public virtual void Emit(LogEvent logEvent)
    {
        Mutate(logEvent);
        _sink.Emit(logEvent);
    }

    /// <summary>Handles an outer LogException, then SimpleException duplication, then invokes the custom mutator.</summary>
    /// <param name="logEvent">The shared event updated in place.</param>
    protected internal virtual void Mutate(LogEvent logEvent)
    {
        logEvent.HandleLogException();
        logEvent.HandleSimpleException();
        _mutator?.Invoke(logEvent);
    }

    /// <summary>Forwards a failure listener to the wrapped sink when it supports failure notifications.</summary>
    /// <param name="failureListener">The listener installed during initialization, before logging begins.</param>
    public void SetFailureListener(ILoggingFailureListener failureListener)
    {
        Check.NotNull(failureListener);
        if (_sink is ISetLoggingFailureListener forwarding)
            forwarding.SetFailureListener(failureListener);
    }

    /// <summary>Disposes the wrapped sink when it implements <see cref="IDisposable"/>.</summary>
    /// <remarks>Only the first disposal call releases the inner sink. Exceptions raised during disposal propagate to the caller.</remarks>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        (_sink as IDisposable)?.Dispose();
    }

    /// <summary>Disposes the wrapped sink asynchronously when it implements <see cref="IAsyncDisposable"/>.</summary>
    /// <returns>An operation that completes when the wrapped sink has finished disposing.</returns>
    /// <remarks>
    /// Falls back to synchronous disposal when the wrapped sink only implements <see cref="IDisposable"/>.
    /// Completes without disposing a sink that implements neither interface. Exceptions raised during
    /// synchronous or asynchronous disposal propagate to the caller.
    /// Only the first synchronous or asynchronous disposal call releases the inner sink; later calls do nothing.
    /// </remarks>
    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

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
