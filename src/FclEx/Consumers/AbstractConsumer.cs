using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Utils;

namespace FclEx.Consumers
{
    public abstract class AbstractConsumer<TSelf, T> : IDisposable
        where TSelf : AbstractConsumer<TSelf, T>
    {
        protected volatile bool _isDisposed;
        protected volatile bool _isStarted;
        protected volatile bool _isAddingCompleted;
        protected CancellationTokenSource _cts;
        protected BlockingCollection<ProcItem<T>> _items;
        protected bool IsComplete => _items.Count == 0 && _isAddingCompleted;

        public int Count => _items?.Count ?? 0;

        protected event AsyncEventHandler<TSelf, ProcItem<T>> OnExceptionInternal
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
            while (!IsComplete && !_cts.IsCancellationRequested)
            {
                if (!TryGetItem(out var item))
                    continue;

                try
                {
                    await OnConsumeInternal.InvokeAsync((TSelf)this, item).DonotCapture();
                }
                catch (Exception ex)
                {
                    item.AddError(ex);
                    await OnExceptionInternal.InvokeAsync((TSelf)this, item).DonotCapture();
                }
            }
            _isStarted = false;
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

        protected void EnsureNonStarted()
        {
            if (_isStarted)
                throw new InvalidOperationException("The consumer is running now");
        }

        protected void EnsureRunnable()
        {
            EnsureNonDisposed();
            EnsureNonStarted();
        }

        protected void EnsureRunning()
        {
            EnsureNonDisposed();
            EnsureStarted();
        }

        public Task Start()
        {
            EnsureRunnable();
            _cts = new CancellationTokenSource();
            _items = new BlockingCollection<ProcItem<T>>();
            _isStarted = true;
            return Task.Run(Process);
        }

        public virtual void Add(T item)
        {
            EnsureRunning();
            _items.Add(new ProcItem<T>(item));
        }

        public virtual void AddRange(ICollection<T> items)
        {
            EnsureRunning();
            foreach (var item in items)
            {
                Add(item);
            }
        }

        public void CompleteAdding()
        {
            EnsureRunning();
            _isAddingCompleted = true;
        }

        public virtual void Dispose()
        {
            if (!_isDisposed)
            {
                _cts?.Cancel();
                _items?.Dispose();
                _isStarted = false;
                _isDisposed = true;
            }
        }
    }
}
