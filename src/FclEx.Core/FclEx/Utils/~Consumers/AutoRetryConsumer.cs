namespace FclEx.Utils;

public sealed class AutoRetryConsumer<T> : ConsumerBase<AutoRetryConsumer<T>, T>,
    IAsyncConsumer<AutoRetryConsumer<T>, T>,
    IDiscardListener<AutoRetryConsumer<T>, ProcessingItem<T>>,
    IExceptionListener<AutoRetryConsumer<T>, ProcessingItem<T>>
{
    private readonly int _maxRetryTimes;
    private readonly Func<int, TimeSpan> _retryDelay;
    private readonly TimeSpan _takeTimeout;

    public event AsyncEventHandler<AutoRetryConsumer<T>, T> ConsumingHandler = (sender, e) => Task.CompletedTask;
    public event EventHandler<AutoRetryConsumer<T>, ProcessingItem<T>> DiscardHandler = (sender, e) => { };
    public event EventHandler<AutoRetryConsumer<T>, ProcessingItem<T>> ExceptionHandler = (sender, e) => { };

    public AutoRetryConsumer(int maxRetryTimes = 3, Func<int, TimeSpan>? retryDelay = null, TimeSpan takeTimeout = default)
    {
        Check.NotLessThan(maxRetryTimes, 0);
        _maxRetryTimes = maxRetryTimes;
        _retryDelay = retryDelay ?? (x => TimeSpan.Zero);
        _takeTimeout = takeTimeout == default
            ? TimeSpan.FromSeconds(1)
            : takeTimeout.Clamp(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(5));
    }

    private bool TryGetItem(out ProcessingItem<T> item)
    {
        try
        {
            var milliseconds = _takeTimeout.TotalMilliseconds.CastTo<int>();
            if (_items.TryTake(out item, milliseconds, _cts.Token))
                return true;
        }
        catch (OperationCanceledException) { }
        item = default;
        return false;
    }

    protected override async Task ProcessActionAsync()
    {
        if (TryGetItem(out var item) == false)
            return;

        try
        {
            await ConsumingHandler.InvokeAsync(this, item.Item);
            Counter.IncrementConsume();
        }
        catch (Exception ex)
        {
            LogException(ex, $"Error encountered when invoking {nameof(ConsumingHandler)}");

            try
            {
                item = item.AddError(ex);
                ExceptionHandler.Invoke(this, item);

                var delay = _retryDelay(item.ErrorTimes);
                await TaskHelper.Delay(delay, _cts.Token);
            }
            catch (Exception e)
            {
                LogException(e, $"Error encountered when invoking {nameof(ExceptionHandler)}");
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
                    Counter.IncrementDiscard();
                }
            }
            catch (Exception e)
            {
                LogException(e, $"Error encountered when invoking {nameof(DiscardHandler)}");
            }
        }
    }
}