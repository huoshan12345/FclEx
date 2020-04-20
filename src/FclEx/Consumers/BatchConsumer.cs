using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dawn;
using FclEx.Utils;
using Microsoft.Extensions.Logging;
using MoreLinq;

namespace FclEx.Consumers
{
    public sealed class BatchConsumer<T> : AbstractConsumer<BatchConsumer<T>, T>,
        IAsyncConsumer<BatchConsumer<T>, IReadOnlyList<T>>,
        IDiscardListener<BatchConsumer<T>, IReadOnlyList<ProcItem<T>>>,
        IExceptionListener<BatchConsumer<T>, IReadOnlyList<ProcItem<T>>>
    {
        private readonly int _batchSize;
        private readonly int _maxRetryTimes;
        private readonly TimeSpan _batchSecondsTimeout;
        private bool HasTimeout => _batchSecondsTimeout > TimeSpan.Zero;

        public event AsyncEventHandler<BatchConsumer<T>, IReadOnlyList<T>> ConsumingHandler = (sender, list) => Task.CompletedTask;
        public event EventHandler<BatchConsumer<T>, IReadOnlyList<ProcItem<T>>> DiscardHandler = (sender, list) => { };
        public event EventHandler<BatchConsumer<T>, IReadOnlyList<ProcItem<T>>> ExceptionHandler = (sender, list) => { };

        public BatchConsumer(int batchSize, TimeSpan batchTimeout, int maxRetryTimes = 3)
        {
            _batchSize = Guard.Argument(batchSize, nameof(batchSize)).Min(1);
            _batchSecondsTimeout = Guard.Argument(batchTimeout, nameof(batchTimeout)).Min(TimeSpan.Zero);
            _maxRetryTimes = Guard.Argument(maxRetryTimes, nameof(maxRetryTimes)).Min(0);
        }

        private List<ProcItem<T>> GetItems()
        {
            var watch = ValueStopwatch.StartNew();
            var list = new List<ProcItem<T>>(_batchSize);
            var timeout = (HasTimeout ? 1 : 5) * 1000;
            while (!_isDisposed && list.Count < _batchSize)
            {
                try
                {
                    if (_items.TryTake(out var item, timeout, _cts.Token))
                        list.Add(item);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                if (HasTimeout)
                {
                    var seconds = watch.GetElapsedTime();
                    if (seconds >= _batchSecondsTimeout)
                        break;
                }
            }
            return list;
        }

        private List<ProcItem<T>>? HandleException(List<ProcItem<T>> items, Exception ex)
        {
            if (items == null || items.Count == 0)
                return items;

            List<ProcItem<T>>? nextItems = null;
            Counter.IncreException();

            try
            {
                for (var i = 0; i < items.Count; i++)
                {
                    var item = items[i].AddError(ex);
                    items[i] = item;
                    if (item.ErrorTimes <= _maxRetryTimes)
                    {
                        _items.TryAdd(item);
                    }
                    else
                    {
                        nextItems ??= new List<ProcItem<T>>();
                        nextItems.Add(item);
                    }
                }
                ExceptionHandler.Invoke(this, items);
            }
            catch (Exception e)
            {
                Counter.IncreException();
                Logger.LogError(e, $"[{TypeName}]Error encountered when invoking {nameof(HandleException)}: " + e.Message);
            }
            return nextItems;
        }

        private void HandleDiscard(List<ProcItem<T>>? items)
        {
            if (items == null || items.Count == 0)
                return;

            try
            {
                DiscardHandler.Invoke(this, items);
                Counter.IncreDiscard(items.Count);
            }
            catch (Exception e)
            {
                Counter.IncreException();
                Logger.LogError(e, $"[{TypeName}]Error encountered when invoking {nameof(HandleDiscard)}: " + e.Message);
            }
        }

        protected override async Task ProcessAction()
        {
            List<ProcItem<T>>? items = null;
            try
            {
                items = GetItems();
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"[{TypeName}]Error encountered when invoking {nameof(GetItems)}: " + e.Message);
            }

            if (items == null || items.Count == 0)
                return;

            try
            {
                var list = items.Select(m => m.Item).ToList();
                await ConsumingHandler.InvokeAsync(this, list).DonotCapture();
                Counter.IncreConsume(list.Count);
                return;
            }
            catch (Exception ex)
            {
                items = HandleException(items, ex);
            }
            HandleDiscard(items);
        }
    }
}
