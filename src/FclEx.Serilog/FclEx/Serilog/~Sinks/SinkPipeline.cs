using System.Runtime.ExceptionServices;
using System.Threading;

namespace FclEx.Serilog;

// Owns sink groups and gives custom wrappers a borrowed entry point, separating emission from ownership.
internal static class SinkPipeline
{
    public static ILogEventSink CreateGroup(IEnumerable<ILogEventSink> sinks) => new GroupSink(sinks.ToArray());

    public static ILogEventSink Wrap(Func<ILogEventSink, ILogEventSink> wrapSink, Action<LoggerSinkConfiguration> configure)
    {
        var configuration = new LoggerConfiguration();
        var sinks = Fields.LoggerConfiguration_Sinks.GetRequiredValue<List<ILogEventSink>>(configuration);
        ILogEventSink inner;
        try
        {
            configure(configuration.WriteTo);
            inner = CreateGroup(sinks);
        }
        catch (Exception error)
        {
            CleanupAfterFailure(error, sinks);
            throw;
        }

        try
        {
            var wrapper = wrapSink(new BorrowedSink(inner));
            if (wrapper == null)
                throw new InvalidOperationException("The sink wrapper factory returned null.");
            return new WrapperSink(wrapper, inner);
        }
        catch (Exception error)
        {
            CleanupAfterFailure(error, new[] { inner });
            throw;
        }
    }

    private sealed class BorrowedSink(ILogEventSink inner) : ILogEventSink, ISetLoggingFailureListener
    {
        // Deliberately exposes no disposal interfaces: the outer adapter retains ownership.
        public void Emit(LogEvent logEvent) => inner.Emit(logEvent);
        public void SetFailureListener(ILoggingFailureListener listener) => ForwardFailureListener(inner, listener);
    }

    private sealed class GroupSink : ILogEventSink, IDisposable, IAsyncDisposable, ISetLoggingFailureListener
    {
        private readonly ILogEventSink[] _sinks;
        private readonly ILogEventSink _aggregate;
        private int _disposed;

        public GroupSink(ILogEventSink[] sinks)
        {
            _sinks = sinks;
            // Reuse Serilog's fan-out and emission failure isolation; ownership is handled separately below.
            _aggregate = LoggerSinkConfiguration.CreateSink(writeTo =>
            {
                foreach (var sink in sinks)
                    writeTo.Sink(sink);
            });
        }

        public void Emit(LogEvent logEvent) => _aggregate.Emit(logEvent);

        public void SetFailureListener(ILoggingFailureListener listener)
        {
            foreach (var sink in _sinks)
                ForwardFailureListener(sink, listener);
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
                DisposeAll(_sinks);
        }

        public ValueTask DisposeAsync() => Interlocked.Exchange(ref _disposed, 1) == 0
            ? DisposeAllAsync(_sinks) : default;
    }

    private sealed class WrapperSink(ILogEventSink wrapper, ILogEventSink inner)
        : ILogEventSink, IDisposable, IAsyncDisposable, ISetLoggingFailureListener
    {
        private int _disposed;

        public void Emit(LogEvent logEvent) => wrapper.Emit(logEvent);

        public void SetFailureListener(ILoggingFailureListener listener)
        {
            // Respect a wrapper's own failure handling when present; otherwise forward to its inner group.
            ForwardFailureListener(wrapper is ISetLoggingFailureListener ? wrapper : inner, listener);
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
                DisposeAll(new[] { wrapper, inner });
        }

        public ValueTask DisposeAsync() => Interlocked.Exchange(ref _disposed, 1) == 0
            ? DisposeAllAsync(new[] { wrapper, inner }) : default;
    }

    private static void ForwardFailureListener(ILogEventSink sink, ILoggingFailureListener listener)
    {
        if (sink is ISetLoggingFailureListener forwarding)
            forwarding.SetFailureListener(listener);
    }

    private static void DisposeAll(IEnumerable<ILogEventSink> sinks)
    {
        List<Exception>? errors = null;
        foreach (var sink in sinks)
        {
            try { (sink as IDisposable)?.Dispose(); }
            catch (Exception error) { (errors ??= []).Add(error); }
        }
        ThrowDisposalErrors(errors);
    }

    private static async ValueTask DisposeAllAsync(IEnumerable<ILogEventSink> sinks)
    {
        List<Exception>? errors = null;
        foreach (var sink in sinks)
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
        if (errors?.Count == 1)
            ExceptionDispatchInfo.Capture(errors[0]).Throw();
        if (errors?.Count > 1)
            throw new AggregateException("Multiple sink disposal operations failed.", errors);
    }

    private static void CleanupAfterFailure(Exception configurationError, IEnumerable<ILogEventSink> sinks)
    {
        try
        {
            // Construction is synchronous, but previously registered sinks may require asynchronous cleanup.
            Task.Run(async () => await DisposeAllAsync(sinks).ConfigureAwait(false)).GetAwaiter().GetResult();
        }
        catch (Exception cleanupError)
        {
            throw new AggregateException("Sink configuration and cleanup both failed.", configurationError, cleanupError);
        }
    }
}
