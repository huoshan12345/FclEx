namespace FclEx.Utils;

/// <summary>
/// Runs a non-overlapping asynchronous callback after an initial delay and then at a fixed delay between executions.
/// </summary>
/// <remarks>
/// The timer is single-use and does not start in its constructor. Call <see cref="RunAsync"/> to start it and retain
/// the returned task, or observe <see cref="Completion"/> after starting. The period is measured from the completion
/// of one callback to the start of the next, so slow callbacks never overlap.
/// </remarks>
public sealed class AsyncTimer : IAsyncDisposable
{
    private readonly object _sync = new();
    private readonly Func<CancellationToken, Task> _callback;
    private readonly Func<Exception, CancellationToken, Task>? _handleExceptionAsync;
    private CancellationTokenSource? _stopCancellation;
    private CancellationTokenSource? _runCancellation;
    private Task? _completion;
    private Task? _disposeTask;
    private bool _started;
    private bool _disposed;

    /// <summary>
    /// Initializes an asynchronous timer without starting it.
    /// </summary>
    /// <param name="callback">The asynchronous callback. It receives the timer's linked cancellation token.</param>
    /// <param name="dueTime">The delay before the first callback. It cannot be negative.</param>
    /// <param name="period">The delay after one callback completes before the next begins. It must be positive.</param>
    /// <param name="handleExceptionAsync">
    /// An optional asynchronous exception handler. When it completes successfully the timer continues; when it is
    /// absent, or when it throws, the timer stops and its completion task faults.
    /// </param>
    public AsyncTimer(
        Func<CancellationToken, Task> callback,
        TimeSpan dueTime,
        TimeSpan period,
        Func<Exception, CancellationToken, Task>? handleExceptionAsync = null)
    {
        _callback = Check.NotNull(callback);
        DueTime = Check.NotLessThan(dueTime, TimeSpan.Zero);
        Period = Check.GreaterThan(period, TimeSpan.Zero);
        _handleExceptionAsync = handleExceptionAsync;
    }

    /// <summary>Gets the delay before the first callback.</summary>
    public TimeSpan DueTime { get; }

    /// <summary>Gets the fixed delay between the completion of one callback and the start of the next.</summary>
    public TimeSpan Period { get; }

    /// <summary>Gets whether the timer has started and its run has not yet completed.</summary>
    public bool IsRunning
    {
        get
        {
            lock (_sync)
                return _completion is { IsCompleted: false };
        }
    }

    /// <summary>
    /// Gets the timer's lifetime task after <see cref="RunAsync"/> has been called.
    /// </summary>
    /// <exception cref="InvalidOperationException">The timer has not been started.</exception>
    public Task Completion
    {
        get
        {
            lock (_sync)
                return _completion ?? throw new InvalidOperationException("The timer has not been started.");
        }
    }

    /// <summary>
    /// Starts the timer's single run and returns its lifetime task.
    /// </summary>
    /// <param name="cancellationToken">Cancels the run and the active callback.</param>
    /// <returns>
    /// A task that completes when the timer is stopped, is canceled when <paramref name="cancellationToken"/> is
    /// canceled, and faults when the callback or exception handler has an unhandled failure.
    /// </returns>
    public Task RunAsync(CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            EnsureNotDisposed();
            if (_started)
                throw new InvalidOperationException("The timer can only be started once.");

            _started = true;
            _stopCancellation = new CancellationTokenSource();
            _runCancellation = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                _stopCancellation.Token);
            _completion = Task.Run(() => RunLoopAsync(
                _runCancellation.Token,
                cancellationToken,
                _stopCancellation.Token),
                default);
            return _completion;
        }
    }

    /// <summary>
    /// Requests an orderly stop and waits for the active callback to observe cancellation and finish.
    /// </summary>
    /// <remarks>Calling this method before <see cref="RunAsync"/> is a no-op.</remarks>
    public async Task StopAsync()
    {
        Task? disposeTask;
        Task? completion;
        CancellationTokenSource? stopCancellation;

        lock (_sync)
        {
            disposeTask = _disposeTask;
            completion = _completion;
            stopCancellation = _stopCancellation;
        }

        if (disposeTask is not null)
        {
            await disposeTask.NoCapture();
            return;
        }

        if (completion is null)
            return;

        ExceptionDispatchInfo? cancellationFailure = null;
        try
        {
            await stopCancellation!.CancelAsync();
        }
        catch (Exception ex)
        {
            cancellationFailure = ExceptionDispatchInfo.Capture(ex);
        }

        await completion.NoCapture();
        cancellationFailure?.Throw();
    }

    /// <summary>Stops the timer, waits for its run to finish, and releases its cancellation resources.</summary>
    public ValueTask DisposeAsync()
    {
        Task? disposeTask;
        Task? completion;
        CancellationTokenSource? stopCancellation;
        CancellationTokenSource? runCancellation;
        TaskCompletionSource? disposalCompletion = null;

        lock (_sync)
        {
            disposeTask = _disposeTask;
            if (disposeTask is null)
            {
                disposalCompletion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                disposeTask = disposalCompletion.Task;
                _disposeTask = disposeTask;
                _disposed = true;
            }

            completion = _completion;
            stopCancellation = _stopCancellation;
            runCancellation = _runCancellation;
        }

        if (disposalCompletion is not null)
        {
            _ = CompleteDisposalAsync(
                completion,
                stopCancellation,
                runCancellation,
                disposalCompletion);
        }

        return new ValueTask(disposeTask);
    }

    private async Task RunLoopAsync(
        CancellationToken cancellationToken,
        CancellationToken callerCancellationToken,
        CancellationToken stopCancellationToken)
    {
        try
        {
            if (DueTime > TimeSpan.Zero)
                await Task.Delay(DueTime, cancellationToken).NoCapture();

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    await _callback(cancellationToken).NoCapture();
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex) when (_handleExceptionAsync is not null)
                {
                    await _handleExceptionAsync(ex, cancellationToken).NoCapture();
                }

                await Task.Delay(Period, cancellationToken).NoCapture();
            }
        }
        catch (OperationCanceledException) when (
            stopCancellationToken.IsCancellationRequested &&
            !callerCancellationToken.IsCancellationRequested)
        {
            // StopAsync and DisposeAsync are orderly completion; caller cancellation remains observable.
        }
    }

    private static async Task CompleteDisposalAsync(
        Task? completion,
        CancellationTokenSource? stopCancellation,
        CancellationTokenSource? runCancellation,
        TaskCompletionSource disposalCompletion)
    {
        Exception? failure = null;
        try
        {
            try
            {
                if (stopCancellation is not null)
                    await stopCancellation.CancelAsync();
            }
            catch (Exception ex)
            {
                failure = ex;
            }

            if (completion is not null)
            {
                try
                {
                    await completion.NoCapture();
                }
                catch (OperationCanceledException)
                {
                    // Cancellation is an expected way for a timer run to end during disposal.
                }
                catch (Exception ex)
                {
                    failure ??= ex;
                }
            }
        }
        finally
        {
            runCancellation?.Dispose();
            stopCancellation?.Dispose();
        }

        if (failure is null)
            disposalCompletion.TrySetResult();
        else
            disposalCompletion.TrySetException(failure);
    }

    private void EnsureNotDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(AsyncTimer));
    }
}
