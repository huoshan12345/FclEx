using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Helpers;
using FclEx.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Nito.AsyncEx;

namespace FclEx.Consumers
{
    public abstract class AbstractConsumer<TSelf, T> : IConsumer<T>,
        ICancellationListener<TSelf, IReadOnlyList<T>>
        where TSelf : AbstractConsumer<TSelf, T>
    {
        private ILogger _logger = NullLogger.Instance;
        protected string TypeName { get; }
        protected readonly AsyncLock _locker = new();
        protected readonly BlockingCollection<ProcItem<T>> _items = new();
        protected volatile bool _isRunning;
        protected volatile bool _isAddingCompleted;
        protected volatile bool _isDisposed;
        protected bool IsCompleteNoLock => (_isDisposed || _items.Count == 0) && _isAddingCompleted;
        protected CancellationTokenSource _cts = new();

        [AllowNull]
        public ILogger Logger
        {
            get => _logger;
            set
            {
                value ??= NullLogger.Instance;
                _logger = value;
            }
        }
        public Counter Counter { get; } = new();
        public int Count => _locker.Do(() => _isDisposed ? 0 : _items.Count);
        public bool IsComplete => _locker.Do(() => IsCompleteNoLock);
        public event EventHandler<TSelf, IReadOnlyList<T>> CancellationHandler = (sender, list) => { };

        protected AbstractConsumer()
        {
            TypeName = GetType().ShortName();
        }

        protected virtual void HandleCancelation()
        {
            if (_isDisposed)
                return;

            var list = new List<T>();
            try
            {
                while (!_isDisposed && _items.TryTake(out var item))
                    list.Add(item.Item);
            }
            catch (Exception ex)
            {
                Counter.IncreException();
                Logger.LogError(ex, $"[{TypeName}]Error encountered when invoking {nameof(_items.TryTake)}: " + ex.Message);
            }

            if (list.IsEmpty())
                return;

            try
            {
                CancellationHandler.Invoke((TSelf)this, list);
            }
            catch (Exception ex)
            {
                Counter.IncreException();
                Logger.LogError(ex, $"[{TypeName}]Error encountered when invoking {nameof(HandleCancelation)}: " + ex.Message);
            }
        }

        protected abstract Task ProcessAction();

        protected virtual async Task Process()
        {
            try
            {
                while (!IsCompleteNoLock && !_cts.IsCancellationRequested)
                    await ProcessAction().DonotCapture();
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

        protected void EnsureNotCompleteAdding()
        {
            if (_isAddingCompleted)
                throw new InvalidOperationException("The consumer has been marked as complete with regards to additions.");
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
                if (_cts.IsCancellationRequested)
                    _cts = new CancellationTokenSource();
                _isRunning = true;
                return Task.Run(Process);
            }
        }

        public virtual void Add(T item)
        {
            EnsureNotCompleteAdding();
            AddWithoutCheckingCompleteAdding(item);
        }

        internal virtual void AddWithoutCheckingCompleteAdding(T item)
        {
            EnsureNonDisposed();
            _items.Add(new ProcItem<T>(item));
        }

        public virtual void CompleteAdding()
        {
            _locker.Do(() => _isAddingCompleted = true);
        }

        public virtual void Stop()
        {
            using (_locker.Lock())
            {
                EnsureNonDisposed();
                EnsureRunnning();
                _cts.Cancel();
                HandleCancelation();
                _isRunning = false;
            }
        }

        public virtual void Dispose()
        {
            if (_isDisposed)
                return;

            _cts.Cancel();
            HandleCancelation();

            _cts.Dispose();
            _items.Dispose();

            _isRunning = false;
            _isDisposed = true;
        }
    }
}
