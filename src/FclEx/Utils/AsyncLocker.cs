using System;
using System.Threading;
using System.Threading.Tasks;

namespace FclEx.Utils
{
    public sealed class AsyncLocker : IDisposable
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        private ActionDisposable Release()
        {
            return new ActionDisposable(() => _semaphore.Release());
        }

        public async Task<IDisposable> LockAsync(CancellationToken token = default)
        {
            await _semaphore.WaitAsync(token).DonotCapture();
            return Release();
        }

        public async Task<IDisposable> LockAsync(TimeSpan span)
        {
            await _semaphore.WaitAsync(span).DonotCapture();
            return Release();
        }

        public IDisposable Lock(CancellationToken token = default)
        {
            _semaphore.Wait(token);
            return Release();
        }

        public IDisposable Lock(TimeSpan span)
        {
            _semaphore.Wait(span);
            return Release();
        }

        public void Dispose()
        {
            _semaphore.Dispose();
        }
    }
}
