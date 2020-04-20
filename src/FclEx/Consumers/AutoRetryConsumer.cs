using System;
using System.Threading.Tasks;
using Dawn;
using FclEx.Helpers;
using FclEx.Utils;
using Microsoft.Extensions.Logging;

namespace FclEx.Consumers
{
    public sealed class AutoRetryConsumer<T> : AbstractConsumer<AutoRetryConsumer<T>, T>,
        IAsyncConsumer<AutoRetryConsumer<T>, T>,
        IDiscardListener<AutoRetryConsumer<T>, ProcItem<T>>,
        IExceptionListener<AutoRetryConsumer<T>, ProcItem<T>>
    {
        private readonly int _maxRetryTimes;
        private readonly Func<int, int> _retryDelay;

        public event AsyncEventHandler<AutoRetryConsumer<T>, T> ConsumingHandler = (sender, e) => Task.CompletedTask;
        public event EventHandler<AutoRetryConsumer<T>, ProcItem<T>> DiscardHandler = (sender, e) => { };
        public event EventHandler<AutoRetryConsumer<T>, ProcItem<T>> ExceptionHandler = (sender, e) => { };

        public AutoRetryConsumer(int maxRetryTimes = 3, Func<int, int>? retryDelay = null)
        {
            Guard.Argument(maxRetryTimes, nameof(maxRetryTimes)).Min(0);
            _maxRetryTimes = maxRetryTimes;
            _retryDelay = retryDelay ?? (x => x);
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

        protected override async Task ProcessAction()
        {
            if (!TryGetItem(out var item))
                return;

            try
            {
                var delay = _retryDelay(item.ErrorTimes);
                await TaskHelper.Delay(delay);
                await ConsumingHandler.InvokeAsync(this, item.Item).DonotCapture();
                Counter.IncreConsume();
            }
            catch (Exception ex)
            {
                Counter.IncreException();
                try
                {
                    item = item.AddError(ex);
                    ExceptionHandler.Invoke(this, item);
                }
                catch (Exception e)
                {
                    Counter.IncreException();
                    Logger.LogError(e, $"[{TypeName}]Error encountered when invoking {nameof(ExceptionHandler)}: " + e.Message);
                }

                try
                {
                    if (item.ErrorTimes < _maxRetryTimes)
                    {
                        _items.TryAdd(item);
                    }
                    else
                    {
                        DiscardHandler.Invoke(this, item);
                        Counter.IncreDiscard();
                    }
                }
                catch (Exception e)
                {
                    Counter.IncreException();
                    Logger.LogError(e, $"[{TypeName}]Error encountered when invoking {nameof(DiscardHandler)}: " + e.Message);
                }
            }
        }
    }
}
