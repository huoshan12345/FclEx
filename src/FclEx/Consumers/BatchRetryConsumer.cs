using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MoreLinq;

namespace FclEx.Consumers
{
    public sealed class BatchRetryConsumer<T> : IConsumer<T>
    {
        private string TypeName { get; }
        private ILogger _logger = NullLogger.Instance;
        private readonly int _retryPartCount;
        private readonly AutoRetryConsumer<List<T>> _retryConsumer;
        private readonly BatchConsumer<T> _batchConsumer;
        public bool IsComplete => _retryConsumer.IsComplete;
        public int Count => _retryConsumer.Count + _batchConsumer.Count;

        public BatchRetryConsumer(int batchSize,
            int batchSecondsTimeout,
            int maxRetryTimes = 3,
            int maxBatchRetryTimes = 0,
            int retryPartCount = 4)
        {
            TypeName = GetType().ShortName();
            _retryPartCount = Check.AtLeast(retryPartCount, nameof(retryPartCount), 2);
            _batchConsumer = new BatchConsumer<T>(batchSize, batchSecondsTimeout, maxBatchRetryTimes);
            _batchConsumer.OnConsume += async (sender, list) =>
            {
                await OnConsume(this, list).DonotCapture();
                Counter.IncreConsume(list.Count);
            };
            _batchConsumer.OnDiscard += (sender, list) =>
            {
                _retryConsumer.Add(list.Select(m => m.Item).ToList());
                SetRetryCompleteAdding();
            };

            _retryConsumer = new AutoRetryConsumer<List<T>>(maxRetryTimes, x => 0);
            _retryConsumer.OnConsume += (sender, list) => Retry(list);
            _retryConsumer.OnException += (sender, list) => HandleException(list);
            _retryConsumer.OnDiscard += (sender, list) => HandleDiscard(list);
        }

        public event EventHandler<BatchRetryConsumer<T>, ProcItem<T>> OnException
            = (sender, e) => { };

        public event AsyncEventHandler<BatchRetryConsumer<T>, IReadOnlyList<T>> OnConsume
            = (sender, e) => Task.CompletedTask;

        public event EventHandler<BatchRetryConsumer<T>, ProcItem<T>> OnDiscard = (sender, e) => { };

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
            return Task.WhenAll(_retryConsumer.Start(clear), _batchConsumer.Start(clear));
        }

        public void Add(T item)
        {
            _batchConsumer.Add(item);
        }

        public void CompleteAdding()
        {
            _batchConsumer.CompleteAdding();
            SetRetryCompleteAdding();
        }

        private void SetRetryCompleteAdding()
        {
            if (_batchConsumer.IsComplete)
                _retryConsumer.CompleteAdding();
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
            if (list.Item == null || list.Item.Count < 1) return;
            var procItem = list.ToType(list.Item.First());
            try
            {
                OnDiscard(this, procItem);
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
            if (list.Item == null || list.Item.Count < 1) return;
            var procItem = list.ToType(list.Item.First());
            try
            {
                OnException(this, procItem);
                Counter.IncreException();
            }
            catch (Exception e)
            {
                Counter.IncreException();
                Logger.LogError(e, $"[{TypeName}]Error encountered when invoking {nameof(HandleException)}: " + e.Message);
            }
        }

        private async Task Retry(IReadOnlyList<T> items)
        {
            if (items == null || items.Count == 0) return;
            try
            {
                await OnConsume.InvokeAsync(this, items).DonotCapture();
                Counter.IncreConsume(items.Count);
                return;
            }
            catch
            {
                if (items.Count > 1)
                {
                    items.Batch((int)Math.Ceiling(items.Count / (double)_retryPartCount))
                        .ForEach(m => _retryConsumer.Add(m.ToList()));
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
