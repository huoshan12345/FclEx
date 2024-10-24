namespace FclEx.Consumers;

public abstract class AbstractConsumer<TSelf, T> : IConsumer<T>, ICancellationListener<TSelf, IReadOnlyList<T>>
    where TSelf : AbstractConsumer<TSelf, T>
{
    protected string TypeName { get; }
    protected readonly AsyncLock _locker = new();
    protected readonly BlockingCollection<ProcessingItem<T>> _items = new();
    protected volatile bool _isRunning;
    protected volatile bool _isAddingCompleted;
    protected volatile bool _isDisposed;
    protected bool IsCompleteNoLock => (_isDisposed || _items.Count == 0) && _isAddingCompleted;
    protected CancellationTokenSource _cts = new();

    public ConsumerCounter Counter { get; } = new();
    public int Count => _locker.Do(() => _isDisposed ? 0 : _items.Count);
    public bool IsComplete => _locker.Do(() => IsCompleteNoLock);
    public event EventHandler<TSelf, IReadOnlyList<T>> CancellationHandler = (sender, list) => { };
    public event EventHandler<TSelf, Exception, string> ExceptionLogger = (sender, exception, message) => { };

    protected AbstractConsumer()
    {
        TypeName = GetType().ShortName();
    }

    protected void LogException(Exception ex, string message)
    {
        Counter.IncrementException();
        ExceptionLogger.Invoke((TSelf)this, ex, $"[{TypeName}]" + message);
    }

    protected virtual void HandleCancellation()
    {
        if (_isDisposed)
            return;

        var list = new List<T>();
        try
        {
            while (!_isDisposed && _items.TryTake(out var item))
                list.Add(item.Item);
        }
        catch (Exception ex)
        {
            LogException(ex, $"Error encountered when invoking {nameof(_items.TryTake)}");
        }

        if (list.IsEmpty())
            return;

        try
        {
            CancellationHandler.Invoke((TSelf)this, list);
        }
        catch (Exception ex)
        {
            LogException(ex, $"Error encountered when invoking {nameof(CancellationHandler)}");
        }
    }

    protected abstract Task ProcessActionAsync();

    protected virtual async Task ProcessAsync()
    {
        try
        {
            while (!IsCompleteNoLock && !_cts.IsCancellationRequested)
                await ProcessActionAsync().IgnoreSyncContext();
        }
        catch (Exception ex)
        {
            LogException(ex, $"Error encountered when invoking {nameof(ProcessActionAsync)}");
        }
        finally
        {
            _locker.Do(() => _isRunning = false);
        }
    }

    protected void EnsureNonDisposed()
    {
        if (_isDisposed)
            throw new ObjectDisposedException("The consumer has been disposed already.");
    }

    protected void EnsureRunning()
    {
        if (!_isRunning)
            throw new InvalidOperationException("The consumer is no running");
    }

    protected void EnsureNotRunning()
    {
        if (_isRunning)
            throw new InvalidOperationException("The consumer has been running already.");
    }

    protected void EnsureNotCompleteAdding()
    {
        if (_isAddingCompleted)
            throw new InvalidOperationException("The consumer has been marked as complete with regards to additions.");
    }

    public virtual async Task StartAsync(bool clear = false)
    {
        Task task;
        using (await _locker.LockAsync())
        {
            EnsureNonDisposed();
            EnsureNotRunning();
            if (clear)
            {
                _items.Clear();
            }
            if (_cts.IsCancellationRequested)
                _cts = new CancellationTokenSource();
            _isRunning = true;
            task = Task.Run(ProcessAsync);
        }
        await task; // NOTE: DO NOT await this task in above lock scope
    }

    public virtual void Add(T item)
    {
        EnsureNotCompleteAdding();
        AddWithoutCheckingCompleteAdding(item);
    }

    public virtual void AddWithoutCheckingCompleteAdding(T item)
    {
        EnsureNonDisposed();
        _items.Add(new ProcessingItem<T>(item));
    }

    public virtual void CompleteAdding()
    {
        _locker.Do(() => _isAddingCompleted = true);
    }

    public virtual void Stop()
    {
        using (_locker.Lock())
        {
            EnsureNonDisposed();
            EnsureRunning();
            _cts.Cancel();
            HandleCancellation();
            _isRunning = false;
        }
    }

    public virtual void Dispose()
    {
        if (_isDisposed)
            return;

        GC.SuppressFinalize(this);

        _cts.Cancel();
        HandleCancellation();

        _cts.Dispose();
        _items.Dispose();

        _isRunning = false;
        _isDisposed = true;
    }
}