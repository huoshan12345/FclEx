using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Utils;

namespace FclEx.Consumers
{
    public abstract class AbstractConsumer<TSelf, T> : IDisposable
        where TSelf : AbstractConsumer<TSelf, T>
    {
        protected bool _isDisposed;
        protected bool _isStarted;
        protected CancellationTokenSource _cts;
        protected BlockingCollection<ProcItem<T>> _items;
        protected ManualResetEvent _finish;

        protected event AsyncEventHandler<TSelf, ProcExItem<ProcItem<T>>> OnExceptionInternal
            = (sender, args) => Task.CompletedTask;

        protected event AsyncEventHandler<TSelf, ProcItem<T>> OnConsumeInternal
            = (sender, e) => Task.CompletedTask;

        private bool TryGetItem(out ProcItem<T> item)
        {
            try
            {
                if (_items.TryTake(out item, 30 * 1000, _cts.Token))
                    return true;

            }
            catch (OperationCanceledException) { }

            item = default;
            return false;
        }

        protected virtual async Task Process()
        {
            while (!_items.IsCompleted && !_cts.IsCancellationRequested)
            {
                if (!TryGetItem(out var item))
                    continue;

                try
                {
                    await OnConsumeInternal.InvokeAsync((TSelf)this, item).DonotCapture();
                    item.ErrorTimes = 0;
                }
                catch (Exception ex)
                {
                    item.ErrorTimes++;
                    var args = ProcItem.CreateEx(item, ex, item.ErrorTimes);
                    await OnExceptionInternal.InvokeAsync((TSelf)this, args).DonotCapture();
                }
            }
            _finish.Set();
        }

        protected void EnsureNonDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException("The consumer has been disposed already.");
        }

        protected void EnsureStarted()
        {
            if (!_isStarted)
                throw new InvalidOperationException("The consumer has not been started yet.");
        }

        protected void EnsureAvailable()
        {
            EnsureNonDisposed();
            EnsureStarted();
        }

        public Task Start()
        {
            EnsureNonDisposed();
            _cts = new CancellationTokenSource();
            _finish = new ManualResetEvent(true);
            _items = new BlockingCollection<ProcItem<T>>();
            _isStarted = true;
            return Task.Run(Process);
        }

        public virtual void Add(T item)
        {
            EnsureAvailable();
            _items.Add(new ProcItem<T>(item));
        }

        public virtual void AddRange(ICollection<T> items)
        {
            EnsureAvailable();
            foreach (var item in items)
            {
                Add(item);
            }
        }

        public virtual void Dispose()
        {
            if (_isDisposed)
            {
                _cts?.Cancel();
                _finish?.WaitOne();
                _items?.Dispose();
                _isDisposed = true;
            }
        }
    }
}
