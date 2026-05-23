namespace FclEx.Utils;

[SuppressMessage("ReSharper", "MemberInitializerValueIgnored")]
public sealed class BatchRetryConsumer<T> : IConsumer<T>,
    ICancellationListener<BatchRetryConsumer<T>, IReadOnlyList<T>>,
    IAsyncConsumer<BatchRetryConsumer<T>, IReadOnlyList<T>>,
    IDiscardListener<BatchRetryConsumer<T>, ProcessingItem<T>>,
    IExceptionListener<BatchRetryConsumer<T>, ProcessingItem<T>>
{
    private readonly int _retryPartCount;
    private readonly AutoRetryConsumer<ArraySegment<T>> _retryConsumer;
    private readonly BatchConsumer<T> _batchConsumer;
    private readonly AsyncLock _locker = new();
    private string TypeName { get; }

    public bool IsComplete => _retryConsumer.IsComplete;
    public int Count => _locker.Do(() => _retryConsumer.Count + _batchConsumer.Count);

    public event AsyncEventHandler<BatchRetryConsumer<T>, IReadOnlyList<T>> ConsumingHandler = (sender, list) => Task.CompletedTask;
    public event EventHandler<BatchRetryConsumer<T>, ProcessingItem<T>> DiscardHandler = (sender, list) => { };
    public event EventHandler<BatchRetryConsumer<T>, ProcessingItem<T>> ExceptionHandler = (sender, list) => { };
    public event EventHandler<BatchRetryConsumer<T>, IReadOnlyList<T>> CancellationHandler = (sender, list) => { };
    public event EventHandler<BatchRetryConsumer<T>, Exception, string> ExceptionLogger = (sender, exception, message) => { };

    public BatchRetryConsumer(int batchSize, TimeSpan batchTimeout, int maxRetryTimes = 3, int retryPartCount = 4)
    {
        TypeName = GetType().ShortName();

        _retryPartCount = Check.NotLessThan(retryPartCount, 2);

        _retryConsumer = new AutoRetryConsumer<ArraySegment<T>>(maxRetryTimes, null, batchTimeout);
        _retryConsumer.ConsumingHandler += (sender, list) => RetryAsync(list);
        _retryConsumer.ExceptionHandler += (sender, list) => HandleException(list);
        _retryConsumer.DiscardHandler += (sender, list) => HandleDiscard(list);
        _retryConsumer.CancellationHandler += (sender, list) => CancellationHandler.Invoke(this, list.SelectMany(m => m).ToList());
        _retryConsumer.ExceptionLogger += (_, ex, m) => LogException(ex, m);

        _batchConsumer = new BatchConsumer<T>(batchSize, batchTimeout, 0);
        _batchConsumer.ConsumingHandler += async (sender, list) =>
        {
            await ConsumingHandler.InvokeAsync(this, list);
            Counter.IncrementConsume(list.Count);
        };
        _batchConsumer.DiscardHandler += (sender, list) => _retryConsumer.Add(list.Select(m => m.Item).ToArray().ToSegment());
        _batchConsumer.CancellationHandler += (sender, list) => CancellationHandler.Invoke(this, list);
        _batchConsumer.ExceptionLogger += (_, ex, m) => LogException(ex, m);
    }

    public ConsumerCounter Counter { get; } = new();

    public Task StartAsync(bool clear = false)
    {
        return Task.WhenAll(
            _retryConsumer.StartAsync(clear),
            _batchConsumer.StartAsync(clear)
                .ContinueWith(t => _retryConsumer.CompleteAdding(), TaskScheduler.Current));
    }

    public void Add(T item)
    {
        _batchConsumer.Add(item);
    }

    public void CompleteAdding()
    {
        _batchConsumer.CompleteAdding();
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

    private void HandleDiscard(ProcessingItem<ArraySegment<T>> item)
    {
        if (item.Item.IsNullOrEmpty())
            return;

        var procItem = item.ToType(item.Item.First());
        try
        {
            DiscardHandler.Invoke(this, procItem);
            Counter.IncrementDiscard();
        }
        catch (Exception e)
        {
            LogException(e, $"Error encountered when invoking {nameof(HandleDiscard)}");
        }
    }

    private void LogException(Exception ex, string message)
    {
        Counter.IncrementException();
        ExceptionLogger.Invoke(this, ex, $"[{TypeName}]" + message);
    }

    private void HandleException(ProcessingItem<ArraySegment<T>> item)
    {
        if (item.Item.IsNullOrEmpty())
            return;

        var procItem = item.ToType(item.Item.First());
        try
        {
            ExceptionHandler.Invoke(this, procItem);
        }
        catch (Exception e)
        {
            LogException(e, $"Error encountered when invoking {nameof(ExceptionHandler)}");
        }
    }

    private async Task RetryAsync(ArraySegment<T> items)
    {
        if (items.IsNullOrEmpty())
            return;

        try
        {
            await ConsumingHandler.InvokeAsync(this, items);
            Counter.IncrementConsume(items.Count);
            return;
        }
        catch
        {
            if (items.Count > 1)
            {
                var size = (int)Math.Ceiling(items.Count / (double)_retryPartCount);
                items.Segments(size).ForEach(m => _retryConsumer.AddWithoutCheckingCompleteAdding(m));
                return;
            }
            else
            {
                throw;
            }
        }
    }
}