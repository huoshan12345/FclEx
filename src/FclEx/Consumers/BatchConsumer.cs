using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Utils;
using Microsoft.Extensions.Logging;
using MoreLinq;

namespace FclEx.Consumers
{
    public class BatchConsumer<T> : AbstractConsumer<BatchConsumer<T>, T>
    {
        private readonly int _maxRetryTimes;
        private readonly int _batchSecondsTimeout;
        private readonly int _batchSize;
        private bool HasTimeout => _batchSecondsTimeout > 0;

        public BatchConsumer(int batchSize, int batchSecondsTimeout, int maxRetryTimes = 3)
        {
            if (batchSize < 1) throw new ArgumentOutOfRangeException(nameof(batchSize));
            _batchSize = batchSize;

            if (batchSecondsTimeout < 0) throw new ArgumentOutOfRangeException(nameof(batchSecondsTimeout));
            _batchSecondsTimeout = batchSecondsTimeout;
            _maxRetryTimes = maxRetryTimes;
        }

        public event EventHandler<BatchConsumer<T>, IReadOnlyList<ProcItem<T>>> OnException
            = (sender, e) => { };

        public event AsyncEventHandler<BatchConsumer<T>, IReadOnlyList<T>> OnConsume
            = (sender, e) => Task.CompletedTask;

        public event EventHandler<BatchConsumer<T>, IReadOnlyList<ProcItem<T>>> OnDiscard = (sender, e) => { };

        private List<ProcItem<T>> GetItems()
        {
            var watch = ValueStopwatch.StartNew();
            var list = new List<ProcItem<T>>(_batchSize);
            var timeout = (HasTimeout ? 1 : 5) * 1000;
            while (list.Count < _batchSize)
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
                    var seconds = watch.GetElapsedTime().TotalSeconds;
                    if (seconds >= _batchSecondsTimeout) break;
                }
            }
            return list;
        }

        private async Task Consume(List<ProcItem<T>> items)
        {
            try
            {
                if (items.IsNullOrEmpty()) return;
                var list = items.Select(m => m.Item).ToArray();
                await OnConsume.InvokeAsync(this, list).DonotCapture();
                Counter.IncreConsume(list.Length);
                return;
            }
            catch (Exception ex)
            {
                Counter.IncreException();
                try
                {
                    for (var i = 0; i < items.Count; i++)
                        items[i] = items[i].AddError(ex);
                    OnException.Invoke(this, items);
                }
                catch (Exception e)
                {
                    Counter.IncreException();
                    Logger.LogError(e, $"[{TypeName}]Error encountered when invoking {nameof(OnException)}: " + e.Message);
                }
            }

            try
            {
                var (retry, discard) = items.PartitionToArray(m => m.ErrorTimes <= _maxRetryTimes);
                retry.ForEach(m => _items.TryAdd(m));
                if (discard.Any())
                {
                    OnDiscard.Invoke(this, discard);
                    Counter.IncreDiscard(discard.Length);
                }
            }
            catch (Exception e)
            {
                Counter.IncreException();
                Logger.LogError(e, $"[{TypeName}]Error encountered when invoking {nameof(Consume)}: " + e.Message);
            }
        }

        protected override async Task Process()
        {
            try
            {
                while (!IsComplete && !_cts.IsCancellationRequested)
                {
                    var items = GetItems();
                    await Consume(items).DonotCapture();
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
    }
}
