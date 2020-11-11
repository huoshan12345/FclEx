using FclEx.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dawn;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MoreLinq;
using Nito.AsyncEx;

namespace FclEx.Consumers
{
    public sealed class BatchRetryConsumer<T> : IConsumer<T>,
        ICancellationListener<BatchRetryConsumer<T>, IReadOnlyList<T>>,
        IAsyncConsumer<BatchRetryConsumer<T>, IReadOnlyList<T>>,
        IDiscardListener<BatchRetryConsumer<T>, ProcItem<T>>,
        IExceptionListener<BatchRetryConsumer<T>, ProcItem<T>>
    {
        private string TypeName { get; }
        private ILogger _logger = NullLogger.Instance;
        private readonly int _retryPartCount;
        private readonly AutoRetryConsumer<List<T>> _retryConsumer;
        private readonly BatchConsumer<T> _batchConsumer;
        private readonly AsyncLock _locker = new AsyncLock();

        public bool IsComplete => _retryConsumer.IsComplete;
        public int Count => _locker.Do(() => _retryConsumer.Count + _batchConsumer.Count);

        public event AsyncEventHandler<BatchRetryConsumer<T>, IReadOnlyList<T>> ConsumingHandler = (sender, list) => Task.CompletedTask;
        public event EventHandler<BatchRetryConsumer<T>, ProcItem<T>> DiscardHandler = (sender, list) => { };
        public event EventHandler<BatchRetryConsumer<T>, ProcItem<T>> ExceptionHandler = (sender, list) => { };
        public event EventHandler<BatchRetryConsumer<T>, IReadOnlyList<T>> CancellationHandler = (sender, list) => { };

        public BatchRetryConsumer(int batchSize, TimeSpan batchTimeout, int maxRetryTimes = 3, int retryPartCount = 4)
        {
            _retryPartCount = Guard.Argument(retryPartCount, nameof(retryPartCount)).Min(2);
            TypeName = GetType().ShortName();

            _retryConsumer = new AutoRetryConsumer<List<T>>(maxRetryTimes, x => 0);
            _retryConsumer.ConsumingHandler += (sender, list) => Retry(list);
            _retryConsumer.ExceptionHandler += (sender, list) => HandleException(list);
            _retryConsumer.DiscardHandler += (sender, list) => HandleDiscard(list);
            _retryConsumer.CancellationHandler += (sender, list) => CancellationHandler.Invoke(this, list.SelectMany(m => m).ToList());

            _batchConsumer = new BatchConsumer<T>(batchSize, batchTimeout, 0);
            _batchConsumer.ConsumingHandler += async (sender, list) =>
            {
                await ConsumingHandler.InvokeAsync(this, list).DonotCapture();
                Counter.IncreConsume(list.Count);
            };
            _batchConsumer.DiscardHandler += (sender, list) => _retryConsumer.Add(list.Select(m => m.Item).ToList());
            _batchConsumer.CancellationHandler += (sender, list) => CancellationHandler.Invoke(this, list);
        }

        public ILogger Logger
        {
            get => _logger;
            set
            {
                if (value != null && value != _logger)
                {
                    _logger = value;
                    _retryConsumer.Logger = value;
                    _batchConsumer.Logger = value;
                }
            }
        }

        public Counter Counter { get; } = new Counter();

        public Task Start(bool clear = false)
        {
            return Task.WhenAll(_retryConsumer.Start(clear), _batchConsumer.Start(clear).ContinueWith(t => _retryConsumer.CompleteAdding()));
        }

        public void Add(T item)
        {
            _batchConsumer.Add(item);
        }

        public void CompleteAdding()
        {
            _batchConsumer.CompleteAdding();
        }

        public void Dispose()
        {
            _retryConsumer.Dispose();
            _batchConsumer.Dispose();
        }

        public void Stop()
        {
            _retryConsumer.Stop();
            _batchConsumer.Stop();
        }

        private void HandleDiscard(ProcItem<List<T>> list)
        {
            if (list.Item == null || list.Item.Count == 0) return;
            var procItem = list.ToType(list.Item.First());
            try
            {
                DiscardHandler.Invoke(this, procItem);
                Counter.IncreDiscard();
            }
            catch (Exception e)
            {
                Counter.IncreException();
                Logger.LogError(e, $"[{TypeName}]Error encountered when invoking {nameof(HandleDiscard)}: " + e.Message);
            }
        }

        private void HandleException(ProcItem<List<T>> list)
        {
            if (list.Item == null || list.Item.Count == 0) return;
            var procItem = list.ToType(list.Item.First());
            try
            {
                ExceptionHandler.Invoke(this, procItem);
                Counter.IncreException();
            }
            catch (Exception e)
            {
                Counter.IncreException();
                Logger.LogError(e, $"[{TypeName}]Error encountered when invoking {nameof(HandleException)}: " + e.Message);
            }
        }

        private async Task Retry(IReadOnlyList<T>? items)
        {
            if (items == null || items.Count == 0) return;
            try
            {
                await ConsumingHandler.InvokeAsync(this, items).DonotCapture();
                Counter.IncreConsume(items.Count);
                return;
            }
            catch
            {
                if (items.Count > 1)
                {
                    items.Batch((int)Math.Ceiling(items.Count / (double)_retryPartCount))
                        .ForEach(m => _retryConsumer.AddWithoutCheckingCompleteAdding(m.ToList()));
                    return;
                }
                else
                {
                    throw;
                }
            }
        }
    }
}

