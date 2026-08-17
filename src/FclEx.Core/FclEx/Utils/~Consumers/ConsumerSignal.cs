namespace FclEx.Utils;

/// <summary>
/// Coalesces queue-state changes into a single asynchronous wake-up signal.
/// </summary>
internal sealed class ConsumerSignal : IDisposable
{
    private readonly SemaphoreSlim _semaphore = new(0, 1);

    public Task WaitAsync(CancellationToken cancellationToken)
        => _semaphore.WaitAsync(cancellationToken);

    public Task<bool> WaitAsync(TimeSpan timeout, CancellationToken cancellationToken)
        => _semaphore.WaitAsync(timeout, cancellationToken);

    public void Pulse()
    {
        try
        {
            _semaphore.Release();
        }
        catch (SemaphoreFullException)
        {
            // A pending signal already represents the latest queue state.
        }
        catch (ObjectDisposedException)
        {
            // A producer that entered before disposal may publish its final state change afterward.
        }
    }

    public void Dispose() => _semaphore.Dispose();
}
