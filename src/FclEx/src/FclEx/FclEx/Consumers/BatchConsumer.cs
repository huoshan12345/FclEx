namespace FclEx.Consumers;

public sealed class BatchConsumer<T> : AbstractConsumer<BatchConsumer<T>, T>,
    IAsyncConsumer<BatchConsumer<T>, IReadOnlyList<T>>,
    IDiscardListener<BatchConsumer<T>, IReadOnlyList<ProcItem<T>>>,
    IExceptionListener<BatchConsumer<T>, IReadOnlyList<ProcItem<T>>>
{
    private readonly int _batchSize;
    private readonly int _maxRetryTimes;
    private readonly TimeSpan _batchTimeout;
    private bool HasTimeout => _batchTimeout > TimeSpan.Zero;

    public event AsyncEventHandler<BatchConsumer<T>, IReadOnlyList<T>> ConsumingHandler = (sender, list) => Task.CompletedTask;
    public event EventHandler<BatchConsumer<T>, IReadOnlyList<ProcItem<T>>> DiscardHandler = (sender, list) => { };
    public event EventHandler<BatchConsumer<T>, IReadOnlyList<ProcItem<T>>> ExceptionHandler = (sender, list) => { };

    public BatchConsumer(int batchSize, TimeSpan batchTimeout, int maxRetryTimes = 3)
    {
        _batchSize = Check.GreaterThan(batchSize, 0);
        _batchTimeout = Check.NotLessThan(batchTimeout, TimeSpan.Zero);
        _maxRetryTimes = Check.NotLessThan(maxRetryTimes, 0);
    }

    // ReSharper disable once InconsistentNaming
    private List<ProcItem<T>> GetItems()
    {
        var watch = ValueStopwatch.StartNew();
        var list = new List<ProcItem<T>>(_batchSize);
        while (!_isDisposed && list.Count < _batchSize)
        {
            try
            {
                if (_items.TryTake(out var item, 1 * 1000, _cts.Token))
                    list.Add(item);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            if (HasTimeout)
            {
                var elapsedTime = watch.GetElapsedTime();
                if (elapsedTime >= _batchTimeout)
                    break;
            }
        }
        return list;
    }

    private List<ProcItem<T>>? HandleException(List<ProcItem<T>> items, Exception ex)
    {
        if (items.IsNullOrEmpty())
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
            await ConsumingHandler.InvokeAsync(this, list).IgnoreSyncContext();
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