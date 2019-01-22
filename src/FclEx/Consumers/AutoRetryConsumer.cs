using System;
using System.Threading.Tasks;
using FclEx.Helpers;
using FclEx.Utils;

namespace FclEx.Consumers
{
    public class AutoRetryConsumer<T> : AbstractConsumer<AutoRetryConsumer<T>, T>
    {
        public event EventHandler<AutoRetryConsumer<T>, ProcItem<T>> OnException = (sender, args) => { };
        public event AsyncEventHandler<AutoRetryConsumer<T>, T> OnConsume = (sender, e) => Task.CompletedTask;
        public event EventHandler<AutoRetryConsumer<T>, ProcItem<T>> OnDiscard = (sender, e) => { };

        public AutoRetryConsumer(int maxRetryTimes, Func<int, int> retryDelay)
        {
            Check.AtLeast(maxRetryTimes, nameof(maxRetryTimes), 0);
            retryDelay = retryDelay ?? (x => x);

            OnConsumeInternal += async (sender, item) =>
            {
                var delay = retryDelay(item.ErrorTimes);
                await TaskHelper.Delay(delay);
                await OnConsume(sender, item.Item).DonotCapture();
            };

            OnExceptionInternal += (sender, args) =>
            {
                OnException.Invoke(sender, args);

                if (args.ErrorTimes < maxRetryTimes)
                    _items.TryAdd(args);
                else
                {
                    OnDiscard.Invoke(sender, args);
                    Counter.IncreDiscard();
                }

                return Task.CompletedTask;
            };
        }

        public AutoRetryConsumer() : this(10, null)
        {
        }
    }
}
