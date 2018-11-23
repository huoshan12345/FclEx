using System;
using System.Threading.Tasks;
using FclEx.Helpers;
using FclEx.Utils;

namespace FclEx.Consumers
{
    public class AutoRetryConsumer<T> : AbstractConsumer<AutoRetryConsumer<T>, T>
    {
        private readonly int _maxRetryTimes;
        private readonly Func<int, int> _retryDelay;

        public event EventHandler<AutoRetryConsumer<T>, ProcExItem<T>> OnException = (sender, args) => { };
        public event AsyncEventHandler<AutoRetryConsumer<T>, T> OnConsume = (sender, e) => Task.CompletedTask;
        public event EventHandler<AutoRetryConsumer<T>, T> OnDiscard = (sender, e) => { };

        public AutoRetryConsumer(int maxRetryTimes, Func<int, int> retryDelay)
        {
            Check.AtLeast(maxRetryTimes, nameof(maxRetryTimes), 0);
            _maxRetryTimes = maxRetryTimes;
            _retryDelay = retryDelay ?? (x => x);

            OnConsumeInternal += async (sender, item) =>
            {
                var delay = _retryDelay(item.ErrorTimes);
                await TaskHelper.Delay(delay);
                await OnConsume(sender, item.Item).DonotCapture();
            };

            OnExceptionInternal += (sender, args) =>
            {
                var item = args.Item;
                OnException.Invoke(sender, ProcItem.CreateEx(item.Item, args.Exception, args.ErrorTimes));

                // 以下是失败后的补救措施
                if (item.ErrorTimes++ < _maxRetryTimes)
                {
                    _items.TryAdd(item);
                }
                else
                {
                    OnDiscard.Invoke(sender, item.Item);
                }

                return Task.CompletedTask;
            };
        }

        public AutoRetryConsumer() : this(10, null)
        {
        }
    }
}
