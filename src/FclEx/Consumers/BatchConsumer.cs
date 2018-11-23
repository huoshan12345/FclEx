using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Utils;
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

        public event EventHandler<BatchConsumer<T>, ProcExItem<IReadOnlyList<ProcItem<T>>>> OnException
            = (sender, e) => { };

        public event AsyncEventHandler<BatchConsumer<T>, IReadOnlyList<T>> OnConsume
            = (sender, e) => Task.CompletedTask;

        public event EventHandler<BatchConsumer<T>, IReadOnlyList<T>> OnDiscard = (sender, e) => { };

        private List<ProcItem<T>> GetItems()
        {
            var startTime = DateTime.UtcNow;
            var list = new List<ProcItem<T>>(_batchSize);
            var timeout = (HasTimeout ? 1 : 60) * 1000;
            while (list.Count < _batchSize)
            {
                try
                {
                    if (_items.TryTake(out var item, timeout, _cts.Token))
                    {
                        list.Add(item);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                if (HasTimeout)
                {
                    var seconds = (int)Math.Ceiling((DateTime.UtcNow - startTime).TotalSeconds);
                    if (seconds >= _batchSecondsTimeout) break;
                }
            }
            return list;
        }

        private async ValueTask Consume(ICollection<ProcItem<T>> items)
        {
            try
            {
                if (items.IsNullOrEmpty()) return;
                var list = items.Select(m => m.Item).ToArray();
                await OnConsume.InvokeAsync(this, list).DonotCapture();
                return;
            }
            catch (Exception ex)
            {
                var list = items.CastTo<IReadOnlyList<ProcItem<T>>>();
                OnException.Invoke(this, ProcItem.CreateEx(list, ex, -1));
            }

            var (retry, discard) = items.Partition(m => m.ErrorTimes < _maxRetryTimes);
            retry.ForEach(m =>
            {
                m.ErrorTimes++;
                _items.TryAdd(m);
            });
            var toDiscard = discard.Select(m => m.Item).ToArray();
            if (toDiscard.Any())
                OnDiscard.Invoke(this, toDiscard);
        }

        protected override async Task Process()
        {
            while (!_items.IsCompleted && !_cts.IsCancellationRequested)
            {
                var items = GetItems();
                await Consume(items).DonotCapture();
            }
            _finish.Set();
        }
    }
}
