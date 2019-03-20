using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FclEx.Consumers
{
    public abstract class AbstractConsumer<TSelf, T> : IDisposable
        where TSelf : AbstractConsumer<TSelf, T>
    {
        public ILogger Logger
        {
            get => _logger;
            set
            {
                if (value != null && value != _logger)
                    _logger = value;
            }
        }
        public Counter Counter { get; } = new Counter();
        protected AsyncLocker Locker = new AsyncLocker();
        protected CancellationTokenSource Cts = new CancellationTokenSource();
        protected BlockingCollection<ProcItem<T>> Items = new BlockingCollection<ProcItem<T>>();
        protected volatile bool IsRunning;
        protected volatile bool IsAddingCompleted;
        protected volatile bool IsDisposed;
        private ILogger _logger = NullLogger.Instance;

        public int Count => Items.Count;
        protected bool IsComplete => Items.Count == 0 && IsAddingCompleted;

        protected event AsyncEventHandler<TSelf, ProcItem<T>> OnExceptionInternal
            = (sender, args) => Task.CompletedTask;

        protected event AsyncEventHandler<TSelf, ProcItem<T>> OnConsumeInternal
            = (sender, e) => Task.CompletedTask;

        private bool TryGetItem(out ProcItem<T> item)
        {
            try
            {
                if (Items.TryTake(out item, 10 * 1000, Cts.Token))
                    return true;
            }
            catch (OperationCanceledException) { }
            item = default;
            return false;
        }

        protected virtual async Task Process()
        {
            try
            {
                while (!IsComplete && !Cts.IsCancellationRequested)
                {
                    if (!TryGetItem(out var item))
                        continue;

                    try
                    {
                        await OnConsumeInternal.InvokeAsync((TSelf)this, item).DonotCapture();
                        Counter.IncreConsume();
                    }
                    catch (Exception ex)
                    {
                        Counter.IncreException();
                        try
                        {
                            item.AddError(ex);
                            await OnExceptionInternal.InvokeAsync((TSelf)this, item).DonotCapture();
                        }
                        catch (Exception e)
                        {
                            Counter.IncreException();
                            Logger.LogError(e, $"[{GetType().Name}]Error encountered when invoking {nameof(OnExceptionInternal)}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Counter.IncreException();
                Logger.LogCritical(e, $"[{GetType().Name}]Error encountered when invoking {nameof(Process)}");
            }
            finally
            {
                Locker.Do(() => IsRunning = false);
            }
        }

        protected void EnsureNonDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException("The consumer has been disposed already.");
        }

        protected void EnsureRunnning()
        {
            if (!IsRunning)
                throw new ObjectDisposedException("The consumer is no running");
        }

        protected void EnsureNotRunnning()
        {
            if (IsRunning)
                throw new ObjectDisposedException("The consumer has been running already.");
        }

        public Task Start()
        {
            using (Locker.Lock())
            {
                EnsureNonDisposed();
                EnsureNotRunnning();
                Items.Clear();
                Cts = new CancellationTokenSource();
                IsRunning = true;
                return Task.Run(Process);
            }
        }

        public virtual void Add(T item)
        {
            Items.Add(new ProcItem<T>(item));
        }

        public virtual void AddRange(ICollection<T> items)
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }

        public void CompleteAdding()
        {
            using (Locker.Lock())
            {
                EnsureRunnning();
                IsAddingCompleted = true;
            }
        }

        public void Stop()
        {
            using (Locker.Lock())
            {
                EnsureNonDisposed();
                if (IsRunning)
                {
                    Cts.Cancel();
                    IsRunning = false;
                }

            }
        }

        public virtual void Dispose()
        {
            Locker.DoubleCheckAndDo(() => !IsDisposed, () =>
             {
                 Cts.Cancel();
                 Items.Dispose();
                 IsRunning = false;
                 IsDisposed = true;
                 GC.SuppressFinalize(this);
             });
        }
    }
}
