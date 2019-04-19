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
    public abstract class AbstractConsumer<TSelf, T> : IConsumer<T>
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
        protected string TypeName { get; }
        protected AsyncLocker _locker = new AsyncLocker();
        protected CancellationTokenSource _cts = new CancellationTokenSource();
        protected BlockingCollection<ProcItem<T>> _items = new BlockingCollection<ProcItem<T>>();
        protected volatile bool _isRunning;
        protected volatile bool _isAddingCompleted;
        protected volatile bool _isDisposed;
        private ILogger _logger = NullLogger.Instance;
        public int Count => _items.Count;
        public bool IsComplete => _items.Count == 0 && _isAddingCompleted;
        protected event AsyncEventHandler<TSelf, ProcItem<T>> OnExceptionInternal
            = (sender, args) => Task.CompletedTask;
        protected event AsyncEventHandler<TSelf, ProcItem<T>> OnConsumeInternal
            = (sender, e) => Task.CompletedTask;

        protected AbstractConsumer()
        {
            TypeName = GetType().ShortName();
        }

        private bool TryGetItem(out ProcItem<T> item)
        {
            try
            {
                if (_items.TryTake(out item, 10 * 1000, _cts.Token))
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
                while (!IsComplete && !_cts.IsCancellationRequested)
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
                            item = item.AddError(ex);
                            await OnExceptionInternal.InvokeAsync((TSelf)this, item).DonotCapture();
                        }
                        catch (Exception e)
                        {
                            Counter.IncreException();
                            Logger.LogError(e, $"[{TypeName}]Error encountered when invoking {nameof(OnExceptionInternal)}: " + e.Message);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Counter.IncreException();
                Logger.LogCritical(e, $"[{TypeName}]Error encountered when invoking {nameof(Process)}: " + e.Message);
            }
            finally
            {
                _locker.Do(() => _isRunning = false);
            }
        }

        protected void EnsureNonDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException("The consumer has been disposed already.");
        }

        protected void EnsureRunnning()
        {
            if (!_isRunning)
                throw new InvalidOperationException("The consumer is no running");
        }

        protected void EnsureNotRunnning()
        {
            if (_isRunning)
                throw new InvalidOperationException("The consumer has been running already.");
        }

        public virtual Task Start(bool clear = false)
        {
            using (_locker.Lock())
            {
                EnsureNonDisposed();
                EnsureNotRunnning();
                if (clear)
                {
                    _items.Clear();
                }
                _cts = new CancellationTokenSource();
                _isRunning = true;
                return Task.Run(Process);
            }
        }

        public virtual void Add(T item)
        {
            _items.Add(new ProcItem<T>(item));
        }

        public virtual void CompleteAdding()
        {
            using (_locker.Lock())
            {
                EnsureRunnning();
                _isAddingCompleted = true;
            }
        }

        public virtual void Stop()
        {
            using (_locker.Lock())
            {
                EnsureNonDisposed();
                if (_isRunning)
                {
                    _cts.Cancel();
                    _isRunning = false;
                }

            }
        }

        public virtual void Dispose()
        {
            _locker.DoubleCheckAndDo(() => !_isDisposed, () =>
             {
                 _cts.Cancel();
                 _items.Dispose();
                 _isRunning = false;
                 _isDisposed = true;
                 GC.SuppressFinalize(this);
             });
        }
    }
}
