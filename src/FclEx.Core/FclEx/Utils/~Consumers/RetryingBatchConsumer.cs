namespace FclEx.Utils;

/// <summary>
/// Consumes producer items in batches and recursively splits failed batches until failing singleton
/// items can be retried and, if necessary, discarded.
/// </summary>
/// <typeparam name="T">The queued item type.</typeparam>
/// <remarks>
/// The consumer is single-use and invokes the consumption delegate serially. A producer batch is
/// consumed when it reaches <see cref="BatchSize"/>, when <see cref="MaxBatchInterval"/> has elapsed
/// since the previous consumption and at least one item is pending, or when adding is completed.
/// Failed batches containing multiple items are split recursively. Retry counting starts only after
/// a failed segment contains one item.
/// All notification events are invoked synchronously on the consumption loop or the caller of <see cref="StopAsync"/>.
/// Handlers must return promptly and must not synchronously wait for the consumer to stop.
/// </remarks>
public sealed class RetryingBatchConsumer<T> : IAsyncDisposable
{
    private readonly object _sync = new();
    private readonly Queue<T> _pendingItems = new();
    private readonly Queue<RetrySegment> _retrySegments = new();
    private readonly ConsumerSignal _signal = new();
    private readonly Func<IReadOnlyList<T>, CancellationToken, Task> _consumeAsync;
    private CancellationTokenSource? _runCancellation;
    private Task _completion = Task.CompletedTask;
    private ValueStopwatch _sinceLastConsumption;
    private int _retryItemCount;
    private bool _started;
    private bool _addingCompleted;
    private bool _stopRequested;
    private bool _completed;
    private bool _disposed;

    private readonly record struct RetrySegment(T[] Items, int SingletonRetryCount);

    private enum WorkState
    {
        Available,
        Wait,
        Complete,
    }

    /// <summary>
    /// Initializes a batch retry consumer.
    /// </summary>
    /// <param name="consumeAsync">The required asynchronous batch consumer.</param>
    /// <param name="batchSize">The maximum number of producer items in one batch.</param>
    /// <param name="maxBatchInterval">The maximum interval between consumption attempts while producer items are pending.</param>
    /// <param name="maxRetryCount">The number of retries allowed after a failed segment reaches one item.</param>
    /// <param name="retryPartitionCount">The target number of segments produced when a multi-item batch fails.</param>
    public RetryingBatchConsumer(
        Func<IReadOnlyList<T>, CancellationToken, Task> consumeAsync,
        int batchSize,
        TimeSpan maxBatchInterval,
        int maxRetryCount = 3,
        int retryPartitionCount = 4)
    {
        _consumeAsync = Check.NotNull(consumeAsync);
        BatchSize = Check.GreaterThan(batchSize, 0);
        MaxBatchInterval = Check.Positive(maxBatchInterval);
        MaxRetryCount = Check.NotLessThan(maxRetryCount, 0);
        RetryPartitionCount = Check.NotLessThan(retryPartitionCount, 2);
    }

    /// <summary>Gets the maximum producer batch size.</summary>
    public int BatchSize { get; }

    /// <summary>Gets the maximum interval between consumption attempts while producer items are pending.</summary>
    public TimeSpan MaxBatchInterval { get; }

    /// <summary>Gets the maximum retry count for a failed singleton segment.</summary>
    public int MaxRetryCount { get; }

    /// <summary>Gets the target partition count used to split failed multi-item batches.</summary>
    public int RetryPartitionCount { get; }

    /// <summary>
    /// Gets the number of pending producer and retry items, excluding a segment currently being consumed.
    /// </summary>
    public int PendingCount
    {
        get
        {
            lock (_sync)
                return _pendingItems.Count + _retryItemCount;
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

    /// <summary>Occurs after a batch or retry segment has been consumed successfully.</summary>
    public event EventHandler<RetryingBatchConsumer<T>, IReadOnlyList<T>>? BatchConsumed;

    /// <summary>Occurs after a batch or retry-segment consumption attempt fails.</summary>
    public event EventHandler<RetryingBatchConsumer<T>, BatchConsumptionFailure<T>>? BatchFailed;

    /// <summary>Occurs when a singleton item is discarded after exhausting its retries.</summary>
    public event EventHandler<RetryingBatchConsumer<T>, DiscardedBatchItem<T>>? ItemDiscarded;

    /// <summary>Occurs when stopping abandons producer items, retry segments, or an active segment.</summary>
    public event EventHandler<RetryingBatchConsumer<T>, IReadOnlyList<T>>? ItemsAbandoned;

    /// <summary>Occurs when a notification listener throws. Listener failures do not affect consumption.</summary>
    public event EventHandler<RetryingBatchConsumer<T>, ConsumerListenerFailure>? ListenerFailed;

    /// <summary>Enqueues one producer item.</summary>
    public void Enqueue(T item)
    {
        lock (_sync)
        {
            EnsureCanEnqueue();
            _pendingItems.Enqueue(item);
        }

        _signal.Pulse();
    }

    /// <summary>Enqueues each producer item in <paramref name="items"/> in enumeration order.</summary>
    public void EnqueueRange(IEnumerable<T> items)
    {
        Check.NotNull(items);
        foreach (var item in items)
            Enqueue(item);
    }

    /// <summary>
    /// Prevents further producer additions and flushes any partial producer batch immediately.
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
            _completion = Task.Run(() => ConsumeLoopAsync(_runCancellation.Token), CancellationToken.None);
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
            if (cancellation is not null)
                await cancellation.CancelAsync().NoCapture();
        }
        catch (Exception ex)
        {
            cancellationFailure = ExceptionDispatchInfo.Capture(ex);
        }

        _signal.Pulse();
        if (abandoned is not null && abandoned.Count > 0)
            Notify(ItemsAbandoned, abandoned.AsReadOnly(), nameof(ItemsAbandoned));

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
        RetrySegment? activeSegment = null;
        _sinceLastConsumption = ValueStopwatch.StartNew();

        try
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var state = TryTakeWork(out var segment, out var waitTimeout);
                if (state == WorkState.Complete)
                    return;

                if (state == WorkState.Wait)
                {
                    if (waitTimeout is { } timeout)
                        _ = await _signal.WaitAsync(timeout, cancellationToken).NoCapture();
                    else
                        await _signal.WaitAsync(cancellationToken).NoCapture();
                    continue;
                }

                activeSegment = segment;
                _sinceLastConsumption = ValueStopwatch.StartNew();
                var items = segment.Items.AsReadOnly();
                try
                {
                    await _consumeAsync(items, cancellationToken).NoCapture();
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Metrics.RecordFailure();
                    var action = HandleConsumptionFailure(segment);
                    Notify(
                        BatchFailed,
                        new BatchConsumptionFailure<T>(
                            items,
                            ex,
                            segment.SingletonRetryCount,
                            action),
                        nameof(BatchFailed));

                    if (action == ConsumerFailureAction.Discard)
                    {
                        Metrics.RecordDiscarded();
                        Notify(
                            ItemDiscarded,
                            new DiscardedBatchItem<T>(
                                items[0],
                                ex,
                                segment.SingletonRetryCount),
                            nameof(ItemDiscarded));
                    }

                    if (action == ConsumerFailureAction.Abandon)
                        break;

                    activeSegment = null;
                    continue;
                }

                Metrics.RecordConsumed(items.Count);
                Notify(BatchConsumed, items, nameof(BatchConsumed));
                activeSegment = null;
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
                if (activeSegment is { } segment)
                    abandoned.InsertRange(0, segment.Items);

                _addingCompleted = true;
                _stopRequested |= cancellationToken.IsCancellationRequested;
                _completed = true;
            }

            if (abandoned.Count > 0)
                Notify(ItemsAbandoned, abandoned.AsReadOnly(), nameof(ItemsAbandoned));
        }
    }

    private WorkState TryTakeWork(out RetrySegment segment, out TimeSpan? waitTimeout)
    {
        lock (_sync)
        {
            if (_retrySegments.Count > 0)
            {
                segment = _retrySegments.Dequeue();
                _retryItemCount -= segment.Items.Length;
                waitTimeout = null;
                return WorkState.Available;
            }

            var pendingCount = _pendingItems.Count;
            if (pendingCount > 0)
            {
                var elapsed = _sinceLastConsumption.GetElapsedTime();
                if (pendingCount >= BatchSize || _addingCompleted || elapsed >= MaxBatchInterval)
                {
                    var count = Math.Min(BatchSize, pendingCount);
                    var items = new T[count];
                    for (var i = 0; i < count; i++)
                        items[i] = _pendingItems.Dequeue();

                    segment = new RetrySegment(items, 0);
                    waitTimeout = null;
                    return WorkState.Available;
                }

                segment = default;
                waitTimeout = MaxBatchInterval - elapsed;
                return WorkState.Wait;
            }

            segment = default;
            waitTimeout = null;
            return _addingCompleted ? WorkState.Complete : WorkState.Wait;
        }
    }

    private ConsumerFailureAction HandleConsumptionFailure(RetrySegment segment)
    {
        ConsumerFailureAction action;
        lock (_sync)
        {
            if (_stopRequested || _runCancellation?.IsCancellationRequested == true)
                return ConsumerFailureAction.Abandon;

            if (segment.Items.Length > 1)
            {
                foreach (var splitSegment in Split(segment.Items))
                {
                    _retrySegments.Enqueue(new RetrySegment(splitSegment, 0));
                    _retryItemCount += splitSegment.Length;
                }
                action = ConsumerFailureAction.Split;
            }
            else if (segment.SingletonRetryCount < MaxRetryCount)
            {
                _retrySegments.Enqueue(new RetrySegment(
                    segment.Items,
                    segment.SingletonRetryCount + 1));
                _retryItemCount++;
                action = ConsumerFailureAction.Retry;
            }
            else
            {
                action = ConsumerFailureAction.Discard;
            }
        }

        if (action is ConsumerFailureAction.Split or ConsumerFailureAction.Retry)
            _signal.Pulse();
        return action;
    }

    private IEnumerable<T[]> Split(T[] items)
    {
        var segmentSize = (int)Math.Ceiling(items.Length / (double)RetryPartitionCount);
        for (var offset = 0; offset < items.Length; offset += segmentSize)
        {
            var length = Math.Min(segmentSize, items.Length - offset);
            var segment = new T[length];
            Array.Copy(items, offset, segment, 0, length);
            yield return segment;
        }
    }

    private List<T> DrainNoLock()
    {
        var items = new List<T>(_pendingItems.Count + _retryItemCount);
        while (_retrySegments.Count > 0)
            items.AddRange(_retrySegments.Dequeue().Items);
        _retryItemCount = 0;

        while (_pendingItems.Count > 0)
            items.Add(_pendingItems.Dequeue());
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
            throw new ObjectDisposedException(nameof(RetryingBatchConsumer<T>));
    }

    private void Notify<TArgs>(
        EventHandler<RetryingBatchConsumer<T>, TArgs>? notification,
        TArgs args,
        string notificationName)
    {
        if (notification is null)
            return;

        foreach (var callback in notification.GetInvocationList())
        {
            try
            {
                ((EventHandler<RetryingBatchConsumer<T>, TArgs>)callback)(this, args);
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
                ((EventHandler<RetryingBatchConsumer<T>, ConsumerListenerFailure>)callback)(this, failure);
            }
            catch
            {
                // A listener-failure observer cannot be allowed to fail the consumer recursively.
            }
        }
    }
}
