using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FclEx.Utils
{
    public class AsyncLocker : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;

        public AsyncLocker(int initialCount = 1)
        {
            Check.AtLeast(initialCount, nameof(initialCount), 1);
            _semaphore = new SemaphoreSlim(initialCount);
        }

        public AsyncLocker(int initialCount, int maxCount)
        {
            Check.AtLeast(initialCount, nameof(initialCount), 1);
            _semaphore = new SemaphoreSlim(initialCount, maxCount);
        }

        public async Task<IDisposable> LockAsync(CancellationToken token = default)
        {
            await _semaphore.WaitAsync(token).DonotCapture();
            return this;
        }

        public async Task<IDisposable> LockAsync(TimeSpan span)
        {
            await _semaphore.WaitAsync(span).DonotCapture();
            return this;
        }

        public IDisposable Lock(CancellationToken token = default)
        {
            _semaphore.Wait(token);
            return this;
        }

        public IDisposable Lock(TimeSpan span)
        {
            _semaphore.Wait(span);
            return this;
        }

        public void Dispose()
        {
            _semaphore.Release();
        }
    }
}
