namespace FclEx.Consumers;

[SuppressMessage("ReSharper", "MemberInitializerValueIgnored")]
public sealed class BatchRetryConsumer<T> : IConsumer<T>,
    ICancellationListener<BatchRetryConsumer<T>, IReadOnlyList<T>>,
    IAsyncConsumer<BatchRetryConsumer<T>, IReadOnlyList<T>>,
    IDiscardListener<BatchRetryConsumer<T>, ProcessingItem<T>>,
    IExceptionListener<BatchRetryConsumer<T>, ProcessingItem<T>>
{
    private string TypeName { get; }
    private ILogger _logger = NullLogger.Instance;
    private readonly int _retryPartCount;
    private readonly AutoRetryConsumer<List<T>> _retryConsumer;
    private readonly BatchConsumer<T> _batchConsumer;
    private readonly AsyncLock _locker = new();

    public bool IsComplete => _retryConsumer.IsComplete;
    public int Count => _locker.Do(() => _retryConsumer.Count + _batchConsumer.Count);

    public event AsyncEventHandler<BatchRetryConsumer<T>, IReadOnlyList<T>> ConsumingHandler = (sender, list) => Task.CompletedTask;
    public event EventHandler<BatchRetryConsumer<T>, ProcessingItem<T>> DiscardHandler = (sender, list) => { };
    public event EventHandler<BatchRetryConsumer<T>, ProcessingItem<T>> ExceptionHandler = (sender, list) => { };
    public event EventHandler<BatchRetryConsumer<T>, IReadOnlyList<T>> CancellationHandler = (sender, list) => { };
    public event EventHandler<BatchRetryConsumer<T>, Exception, string> ExceptionLogger = (sender, exception, message) => { };

    public BatchRetryConsumer(int batchSize, TimeSpan batchTimeout, int maxRetryTimes = 3, int retryPartCount = 4)
    {
        _retryPartCount = Check.NotLessThan(retryPartCount, 2);
        TypeName = GetType().ShortName();

        _retryConsumer = new AutoRetryConsumer<List<T>>(maxRetryTimes);
        _retryConsumer.ConsumingHandler += (sender, list) => RetryAsync(list);
        _retryConsumer.ExceptionHandler += (sender, list) => HandleException(list);
        _retryConsumer.DiscardHandler += (sender, list) => HandleDiscard(list);
        _retryConsumer.CancellationHandler += (sender, list) => CancellationHandler.Invoke(this, list.SelectMany(m => m).ToList());
        _retryConsumer.ExceptionLogger += (_, ex, m) => LogException(ex, m);

        _batchConsumer = new BatchConsumer<T>(batchSize, batchTimeout, 0);
        _batchConsumer.ConsumingHandler += async (sender, list) =>
        {
            await ConsumingHandler.InvokeAsync(this, list).IgnoreSyncContext();
            Counter.IncrementConsume(list.Count);
        };
        _batchConsumer.DiscardHandler += (sender, list) => _retryConsumer.Add(list.Select(m => m.Item).ToList());
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

    private void HandleDiscard(ProcessingItem<List<T>> list)
    {
        if (list.Item.IsNullOrEmpty())
            return;

        var procItem = list.ToType(list.Item.First());
        try
        {
            DiscardHandler.Invoke(this, procItem);
            Counter.IncrementDiscard();
        }
        catch (Exception e)
        {
            Counter.IncrementException();
            LogException(e, $"Error encountered when invoking {nameof(HandleDiscard)}");
        }
    }

    private void LogException(Exception ex, string message)
    {
        Counter.IncrementException();
        ExceptionLogger.Invoke(this, ex, message);
    }

    private void HandleException(ProcessingItem<List<T>> list)
    {
        if (list.Item.IsNullOrEmpty())
            return;

        var procItem = list.ToType(list.Item.First());
        try
        {
            ExceptionHandler.Invoke(this, procItem);
            Counter.IncrementException();
        }
        catch (Exception e)
        {
            Counter.IncrementException();
            LogException(e, $"Error encountered when invoking {nameof(HandleException)}");
        }
    }

    private async Task RetryAsync(IReadOnlyList<T>? items)
    {
        if (items == null || items.Count == 0) return;
        try
        {
            await ConsumingHandler.InvokeAsync(this, items).IgnoreSyncContext();
            Counter.IncrementConsume(items.Count);
            return;
        }
        catch
        {
            if (items.Count > 1)
            {
                items.Chunk((int)Math.Ceiling(items.Count / (double)_retryPartCount))
                    .ForEach(m => _retryConsumer.AddWithoutCheckingCompleteAdding(m.ToList()));
                return;
            }
            else
            {
                throw;
            }
        }
    }
}