using System.Runtime.ExceptionServices;
using System.Threading;

namespace FclEx.Serilog;

// Owns the actual sinks. The outer wrapper owns this group and forwards disposal and failure listeners to it.
internal sealed class AggregateSink : ILogEventSink, IDisposable, IAsyncDisposable, ISetLoggingFailureListener
{
    private readonly ILogEventSink[] _sinks;
    private readonly ILogEventSink _aggregate;
    private int _disposed;

    public AggregateSink(ILogEventSink[] sinks)
    {
        _sinks = sinks;
        // Reuse Serilog's fan-out and emission failure isolation; this class handles ownership and notifications.
        _aggregate = LoggerSinkConfiguration.CreateSink(writeTo =>
        {
            foreach (var sink in sinks)
                writeTo.Sink(sink);
        });
    }

    public void Emit(LogEvent logEvent) => _aggregate.Emit(logEvent);

    public void SetFailureListener(ILoggingFailureListener failureListener)
    {
        foreach (var sink in _sinks)
        {
            if (sink is ISetLoggingFailureListener forwarding)
                forwarding.SetFailureListener(failureListener);
        }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        List<Exception>? errors = null;
        foreach (var sink in _sinks)
        {
            try { (sink as IDisposable)?.Dispose(); }
            catch (Exception error) { (errors ??= []).Add(error); }
        }
        ThrowDisposalErrors(errors);
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        List<Exception>? errors = null;
        foreach (var sink in _sinks)
        {
            try
            {
                if (sink is IAsyncDisposable asynchronous)
                    await asynchronous.DisposeAsync().ConfigureAwait(false);
                else
                    (sink as IDisposable)?.Dispose();
            }
            catch (Exception error) { (errors ??= []).Add(error); }
        }
        ThrowDisposalErrors(errors);
    }

    private static void ThrowDisposalErrors(List<Exception>? errors)
    {
        switch (errors?.Count)
        {
            case 1:
                errors[0].ReThrow();
                break;
            case > 1:
                throw new AggregateException("Multiple sink disposal operations failed.", errors);
        }
    }
}
