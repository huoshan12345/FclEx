namespace FclEx.Consumers;

public sealed class AutoRetryConsumer<T> : AbstractConsumer<AutoRetryConsumer<T>, T>,
    IAsyncConsumer<AutoRetryConsumer<T>, T>,
    IDiscardListener<AutoRetryConsumer<T>, ProcessingItem<T>>,
    IExceptionListener<AutoRetryConsumer<T>, ProcessingItem<T>>
{
    private readonly int _maxRetryTimes;
    private readonly Func<int, TimeSpan> _retryDelay;

    public event AsyncEventHandler<AutoRetryConsumer<T>, T> ConsumingHandler = (sender, e) => Task.CompletedTask;
    public event EventHandler<AutoRetryConsumer<T>, ProcessingItem<T>> DiscardHandler = (sender, e) => { };
    public event EventHandler<AutoRetryConsumer<T>, ProcessingItem<T>> ExceptionHandler = (sender, e) => { };

    public AutoRetryConsumer(int maxRetryTimes = 3, Func<int, TimeSpan>? retryDelay = null)
    {
        Check.NotLessThan(maxRetryTimes, 0);
        _maxRetryTimes = maxRetryTimes;
        _retryDelay = retryDelay ?? (x => TimeSpan.Zero);
    }

    private bool TryGetItem(out ProcessingItem<T> item)
    {
        try
        {
            if (_items.TryTake(out item, 1 * 1000, _cts.Token))
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
            await Task.Delay(delay);
            await ConsumingHandler.InvokeAsync(this, item.Item).IgnoreSyncContext();
            Counter.IncrementConsume();
        }
        catch (Exception ex)
        {
            Counter.IncrementException();
            try
            {
                item = item.AddError(ex);
                ExceptionHandler.Invoke(this, item);
            }
            catch (Exception e)
            {
                Counter.IncrementException();
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
                    Counter.IncrementDiscard();
                }
            }
            catch (Exception e)
            {
                Counter.IncrementException();
                Logger.LogError(e, $"[{TypeName}]Error encountered when invoking {nameof(DiscardHandler)}: " + e.Message);
            }
        }
    }
}