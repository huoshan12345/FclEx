namespace FclEx.Utils;

public class AsyncLock : IDisposable
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public IDisposable Lock(CancellationToken token = default)
    {
        _semaphore.Wait(token);
        return Disposable.Create(() => _semaphore.Release());
    }

    public async Task<IDisposable> LockAsync(CancellationToken token = default)
    {
        await _semaphore.WaitAsync(token).IgnoreSyncContext();
        return Disposable.Create(() => _semaphore.Release());
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _semaphore.Dispose();
    }
}