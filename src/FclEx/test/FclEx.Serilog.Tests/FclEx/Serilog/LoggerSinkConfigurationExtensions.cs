namespace FclEx.Serilog;

public class LogEventListener : ILogEventSink, IDisposable
{
    private readonly SemaphoreSlim _semaphore = new(0);
    public List<LogEvent> Events { get; } = [];

    public virtual void Emit(LogEvent logEvent)
    {
        Events.Add(logEvent);
        _semaphore.Release();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _semaphore.Dispose();
        Events.Clear();
    }

    public Task<bool> WaitAsync(int count, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        return _semaphore.WaitAsync(count, timeout, cancellationToken);
    }
}
