namespace FclEx.Utils;

/// <summary>
/// Consumes queued items one at a time and retries failed items before discarding them.
/// </summary>
/// <typeparam name="T">The queued item type.</typeparam>
/// <remarks>
/// The consumer is single-use. Items may be enqueued before or after <see cref="StartAsync"/>,
/// but not after <see cref="CompleteAdding"/> or stopping begins. Consumption is strictly serial.
/// </remarks>
public sealed class RetryingConsumer<T> : IAsyncDisposable
{
    private readonly object _sync = new();
    private readonly Queue<QueuedItem> _queue = new();
    private readonly ConsumerSignal _signal = new();
    private readonly Func<T, CancellationToken, Task> _consumeAsync;
    private CancellationTokenSource? _runCancellation;
    private Task _completion = Task.CompletedTask;
    private bool _started;
    private bool _addingCompleted;
    private bool _stopRequested;
    private bool _completed;
    private bool _disposed;

    private readonly record struct QueuedItem(T Item, int AttemptNumber);

    /// <summary>
    /// Initializes a single-item retry consumer.
    /// </summary>
    /// <param name="consumeAsync">The required asynchronous item consumer.</param>
    /// <param name="maxRetryCount">The number of retries allowed after the initial attempt.</param>
    public RetryingConsumer(
        Func<T, CancellationToken, Task> consumeAsync,
        int maxRetryCount = 3)
    {
        _consumeAsync = Check.NotNull(consumeAsync);
        MaxRetryCount = Check.NotLessThan(maxRetryCount, 0);
    }

    /// <summary>Gets the maximum number of retries after the initial attempt.</summary>
    public int MaxRetryCount { get; }

    /// <summary>Gets the number of queued items, excluding an item currently being consumed.</summary>
    public int PendingCount
    {
        get
        {
            lock (_sync)
                return _queue.Count;
        }
    }

    /// <summary>Gets whether producers have completed adding items.</summary>
    public bool IsAddingCompleted
    {
        get
        {
            lock (_sync)
                return _addingCompleted;
        }
    }

    /// <summary>Gets whether the consumer's single run has finished.</summary>
    public bool IsCompleted
    {
        get
        {
            lock (_sync)
                return _completed;
        }
    }

    /// <summary>Gets processing counters for this consumer.</summary>
    public ConsumerMetrics Metrics { get; } = new();

    /// <summary>Occurs after an item has been consumed successfully.</summary>
    public event EventHandler<RetryingConsumer<T>, T>? ItemConsumed;

    /// <summary>Occurs after an item-consumption attempt fails.</summary>
    public event EventHandler<RetryingConsumer<T>, ItemConsumptionFailure<T>>? ItemFailed;

    /// <summary>Occurs when an item is discarded after exhausting its retries.</summary>
    public event EventHandler<RetryingConsumer<T>, DiscardedItem<T>>? ItemDiscarded;

    /// <summary>Occurs when stopping abandons queued or active items.</summary>
    public event EventHandler<RetryingConsumer<T>, IReadOnlyList<T>>? ItemsAbandoned;

    /// <summary>Occurs when a notification listener throws. Listener failures do not affect consumption.</summary>
    public event EventHandler<RetryingConsumer<T>, ConsumerListenerFailure>? ListenerFailed;

    /// <summary>Enqueues an item for consumption.</summary>
    public void Enqueue(T item)
    {
        lock (_sync)
        {
            EnsureCanEnqueue();
            _queue.Enqueue(new QueuedItem(item, 1));
        }

        _signal.Pulse();
    }

    /// <summary>Enqueues each item in <paramref name="items"/> in enumeration order.</summary>
    public void EnqueueRange(IEnumerable<T> items)
    {
        Check.NotNull(items);
        foreach (var item in items)
            Enqueue(item);
    }

    /// <summary>
    /// Prevents further producer additions and lets the consumer finish all queued items and retries.
    /// </summary>
    public void CompleteAdding()
    {
        lock (_sync)
        {
            EnsureNotDisposed();
            _addingCompleted = true;
        }

        _signal.Pulse();
    }

    /// <summary>
    /// Starts the consumer's single run and returns a task that completes when graceful completion or stopping finishes.
    /// </summary>
    /// <param name="cancellationToken">Stops the consumer and abandons unfinished items when canceled.</param>
    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            EnsureNotDisposed();
            if (_started)
                throw new InvalidOperationException("The consumer can only be started once.");
            if (_stopRequested)
                throw new InvalidOperationException("The consumer has already been stopped.");

            _started = true;
            _runCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _completion = Task.Run(() => ConsumeLoopAsync(_runCancellation.Token));
            return _completion;
        }
    }

    /// <summary>
    /// Stops the consumer, waits for the active consumer delegate to finish or observe cancellation,
    /// and reports all unfinished items through <see cref="ItemsAbandoned"/>.
    /// </summary>
    public async Task StopAsync()
    {
        CancellationTokenSource? cancellation;
        Task completion;
        List<T>? abandoned = null;

        lock (_sync)
        {
            if (_completed || _disposed)
                return;

            _addingCompleted = true;
            _stopRequested = true;
            cancellation = _runCancellation;
            completion = _completion;

            if (!_started)
            {
                abandoned = DrainNoLock();
                _completed = true;
            }
        }

        ExceptionDispatchInfo? cancellationFailure = null;
        try
        {
            cancellation?.Cancel();
        }
        catch (Exception ex)
        {
            cancellationFailure = ExceptionDispatchInfo.Capture(ex);
        }

        _signal.Pulse();
        if (abandoned is not null && abandoned.Count > 0)
            Notify(ItemsAbandoned, abandoned, nameof(ItemsAbandoned));

        await completion.NoCapture();
        cancellationFailure?.Throw();
    }

    /// <summary>Stops the consumer and releases its synchronization resources.</summary>
    public async ValueTask DisposeAsync()
    {
        CancellationTokenSource? cancellation;
        try
        {
            await StopAsync().NoCapture();
        }
        finally
        {
            var shouldDispose = false;
            lock (_sync)
            {
                if (!_disposed)
                {
                    _disposed = true;
                    shouldDispose = true;
                }
                cancellation = _runCancellation;
            }

            if (shouldDispose)
            {
                cancellation?.Dispose();
                _signal.Dispose();
                GC.SuppressFinalize(this);
            }
        }
    }

    private async Task ConsumeLoopAsync(CancellationToken cancellationToken)
    {
        QueuedItem? activeItem = null;
        try
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!TryDequeue(out var queuedItem))
                {
                    if (ShouldComplete())
                        return;

                    await _signal.WaitAsync(cancellationToken).NoCapture();
                    continue;
                }

                activeItem = queuedItem;
                try
                {
                    await _consumeAsync(queuedItem.Item, cancellationToken).NoCapture();
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Metrics.RecordFailure();
                    var action = HandleConsumptionFailure(queuedItem);
                    Notify(
                        ItemFailed,
                        new ItemConsumptionFailure<T>(queuedItem.Item, ex, queuedItem.AttemptNumber, action),
                        nameof(ItemFailed));

                    if (action == ConsumerFailureAction.Discard)
                    {
                        Metrics.RecordDiscarded();
                        Notify(
                            ItemDiscarded,
                            new DiscardedItem<T>(queuedItem.Item, ex, queuedItem.AttemptNumber),
                            nameof(ItemDiscarded));
                    }

                    if (action == ConsumerFailureAction.Abandon)
                        break;

                    activeItem = null;
                    continue;
                }

                Metrics.RecordConsumed();
                Notify(ItemConsumed, queuedItem.Item, nameof(ItemConsumed));
                activeItem = null;
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Stopping is finalized below so all unfinished items can be reported together.
        }
        finally
        {
            List<T> abandoned;
            lock (_sync)
            {
                abandoned = DrainNoLock();
                if (activeItem is { } item)
                    abandoned.Insert(0, item.Item);

                _addingCompleted = true;
                _stopRequested |= cancellationToken.IsCancellationRequested;
                _completed = true;
            }

            if (abandoned.Count > 0)
                Notify(ItemsAbandoned, abandoned, nameof(ItemsAbandoned));
        }
    }

    private ConsumerFailureAction HandleConsumptionFailure(QueuedItem item)
    {
        bool retry;
        lock (_sync)
        {
            if (_stopRequested || _runCancellation?.IsCancellationRequested == true)
                return ConsumerFailureAction.Abandon;

            retry = item.AttemptNumber <= MaxRetryCount;
            if (retry)
                _queue.Enqueue(new QueuedItem(item.Item, item.AttemptNumber + 1));
        }

        if (retry)
        {
            _signal.Pulse();
            return ConsumerFailureAction.Retry;
        }

        return ConsumerFailureAction.Discard;
    }

    private bool TryDequeue(out QueuedItem item)
    {
        lock (_sync)
        {
            if (_queue.Count > 0)
            {
                item = _queue.Dequeue();
                return true;
            }
        }

        item = default;
        return false;
    }

    private bool ShouldComplete()
    {
        lock (_sync)
            return _addingCompleted && _queue.Count == 0;
    }

    private List<T> DrainNoLock()
    {
        var items = new List<T>(_queue.Count);
        while (_queue.Count > 0)
            items.Add(_queue.Dequeue().Item);
        return items;
    }

    private void EnsureCanEnqueue()
    {
        EnsureNotDisposed();
        if (_addingCompleted || _stopRequested)
            throw new InvalidOperationException("The consumer is no longer accepting items.");
    }

    private void EnsureNotDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RetryingConsumer<T>));
    }

    private void Notify<TArgs>(
        EventHandler<RetryingConsumer<T>, TArgs>? notification,
        TArgs args,
        string notificationName)
    {
        if (notification is null)
            return;

        foreach (var callback in notification.GetInvocationList())
        {
            try
            {
                ((EventHandler<RetryingConsumer<T>, TArgs>)callback)(this, args);
            }
            catch (Exception ex)
            {
                NotifyListenerFailure(notificationName, ex);
            }
        }
    }

    private void NotifyListenerFailure(string notificationName, Exception exception)
    {
        var notification = ListenerFailed;
        if (notification is null)
            return;

        var failure = new ConsumerListenerFailure(notificationName, exception);
        foreach (var callback in notification.GetInvocationList())
        {
            try
            {
                ((EventHandler<RetryingConsumer<T>, ConsumerListenerFailure>)callback)(this, failure);
            }
            catch
            {
                // A listener-failure observer cannot be allowed to fail the consumer recursively.
            }
        }
    }
}
