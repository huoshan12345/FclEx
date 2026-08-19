namespace FclEx.Extensions;

partial class EnumerableExtensions
{
    public static Task WhenAll(this IEnumerable<Task> tasks) => Task.WhenAll(tasks);

    public static Task<T[]> WhenAll<T>(this IEnumerable<Task<T>> tasks) => Task.WhenAll(tasks);

    /// <summary>
    /// Invokes an asynchronous operation for each item in source order, optionally delaying between items.
    /// </summary>
    /// <remarks>Cancellation faults the returned task as canceled; it never returns a successful partial execution.</remarks>
    public static async Task ForEachSequentiallyAsync<T>(
        this IEnumerable<T> source,
        Func<T, CancellationToken, ValueTask> operation,
        TimeSpan delayBetweenItems = default,
        CancellationToken cancellationToken = default)
    {
        Check.NotNull(source);
        Check.NotNull(operation);
        Check.NotLessThan(delayBetweenItems, TimeSpan.Zero);
        cancellationToken.ThrowIfCancellationRequested();

        var isFirstItem = true;
        foreach (var item in source)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (isFirstItem == false && delayBetweenItems > TimeSpan.Zero)
                await Task.Delay(delayBetweenItems, cancellationToken).NoCapture();

            await operation(item, cancellationToken).NoCapture();
            isFirstItem = false;
        }
    }

    /// <summary>
    /// Projects each item asynchronously in source order, optionally delaying between items.
    /// </summary>
    /// <returns>The operation results in source order.</returns>
    /// <remarks>Cancellation faults the returned task as canceled; it never returns partial results.</remarks>
    public static async Task<TResult[]> SelectSequentiallyAsync<T, TResult>(
        this IEnumerable<T> source,
        Func<T, CancellationToken, ValueTask<TResult>> operation,
        TimeSpan delayBetweenItems = default,
        CancellationToken cancellationToken = default)
    {
        Check.NotNull(source);
        Check.NotNull(operation);
        Check.NotLessThan(delayBetweenItems, TimeSpan.Zero);
        cancellationToken.ThrowIfCancellationRequested();

        var results = new List<TResult>();
        var isFirstItem = true;
        foreach (var item in source)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (isFirstItem == false && delayBetweenItems > TimeSpan.Zero)
                await Task.Delay(delayBetweenItems, cancellationToken).NoCapture();

            results.Add(await operation(item, cancellationToken).NoCapture());
            isFirstItem = false;
        }

        return results.ToArray();
    }

    /// <summary>
    /// Invokes an asynchronous operation with a fixed maximum number of active operations.
    /// </summary>
    /// <remarks>
    /// Source is enumerated lazily by the workers. Cancellation or an operation failure stops workers from claiming
    /// additional items and completes the returned task as canceled or faulted rather than reporting partial success.
    /// </remarks>
    public static async Task ForEachConcurrentlyAsync<T>(
        this IEnumerable<T> source,
        Func<T, CancellationToken, ValueTask> operation,
        int maxDegreeOfParallelism,
        CancellationToken cancellationToken = default)
    {
        Check.NotNull(source);
        Check.NotNull(operation);
        Check.Positive(maxDegreeOfParallelism);

        cancellationToken.ThrowIfCancellationRequested();
        using var enumerator = source.GetEnumerator();
        var gate = new object();
        var stopRequested = false;
        var workers = new Task[maxDegreeOfParallelism];

        for (var i = 0; i < workers.Length; i++)
            workers[i] = RunWorkerAsync();

        await Task.WhenAll(workers).NoCapture();
        return;

        async Task RunWorkerAsync()
        {
            try
            {
                while (TryTakeNext(out var item))
                {
                    await operation(item, cancellationToken).NoCapture();
                }
            }
            catch
            {
                Stop();
                throw;
            }
        }

        bool TryTakeNext([MaybeNullWhen(false)] out T item)
        {
            lock (gate)
            {
                if (stopRequested)
                {
                    item = default;
                    return false;
                }

                cancellationToken.ThrowIfCancellationRequested();
                if (enumerator.MoveNext() == false)
                {
                    stopRequested = true;
                    item = default;
                    return false;
                }

                item = enumerator.Current;
                return true;
            }
        }

        void Stop()
        {
            lock (gate)
                stopRequested = true;
        }
    }

    /// <summary>
    /// Projects source items asynchronously with a fixed maximum number of active operations.
    /// </summary>
    /// <returns>All operation results in source order, regardless of completion order.</returns>
    /// <remarks>
    /// Source is enumerated lazily by the workers. Cancellation or an operation failure stops workers from claiming
    /// additional items and completes the returned task as canceled or faulted rather than returning partial results.
    /// </remarks>
    public static async Task<TResult[]> SelectConcurrentlyAsync<T, TResult>(
        this IEnumerable<T> source,
        Func<T, CancellationToken, ValueTask<TResult>> operation,
        int maxDegreeOfParallelism,
        CancellationToken cancellationToken = default)
    {
        Check.NotNull(source);
        Check.NotNull(operation);
        Check.Positive(maxDegreeOfParallelism);

        cancellationToken.ThrowIfCancellationRequested();
        using var enumerator = source.GetEnumerator();
        var gate = new object();
        var results = new List<TResult>();
        var stopRequested = false;
        var workers = new Task[maxDegreeOfParallelism];

        for (var i = 0; i < workers.Length; i++)
            workers[i] = RunWorkerAsync();

        await Task.WhenAll(workers).NoCapture();
        return results.ToArray();

        async Task RunWorkerAsync()
        {
            try
            {
                while (TryTakeNext(out var item, out var index))
                {
                    var result = await operation(item, cancellationToken).NoCapture();
                    lock (gate)
                        results[index] = result;
                }
            }
            catch
            {
                Stop();
                throw;
            }
        }

        bool TryTakeNext([MaybeNullWhen(false)] out T item, out int index)
        {
            lock (gate)
            {
                if (stopRequested)
                {
                    item = default;
                    index = -1;
                    return false;
                }

                cancellationToken.ThrowIfCancellationRequested();
                if (enumerator.MoveNext() == false)
                {
                    stopRequested = true;
                    item = default;
                    index = -1;
                    return false;
                }

                item = enumerator.Current;
                index = results.Count;
                results.Add(default!);
                return true;
            }
        }

        void Stop()
        {
            lock (gate)
                stopRequested = true;
        }
    }

    public static async Task<T> WhenAny<T>(this IEnumerable<Task<T>> tasks)
    {
        return await (await Task.WhenAny(tasks));
    }

    private static async Task<(bool HasResult, T Result)> TryGetFirstSuccessfulResultAsync<T>(
        IEnumerable<Task<T>> tasks,
        Func<T, bool> predicate)
    {
        var remainingTasks = tasks.ToList();
        while (remainingTasks.Count > 0)
        {
            var completedTask = await Task.WhenAny(remainingTasks).NoCapture();
            remainingTasks.Remove(completedTask);

            if (completedTask.Status == TaskStatus.RanToCompletion)
            {
                var result = completedTask.Result;
                if (predicate(result))
                    return (true, result);
            }
            else if (completedTask.IsFaulted)
            {
                _ = completedTask.Exception;
            }
        }

        return (false, default!);
    }

    public static async Task<T> WhenAnySuccess<T>(this IEnumerable<Task<T>> tasks, Func<T, bool> predicate, Func<T> defaultResultFunc)
    {
        Check.NotNull(tasks);
        Check.NotNull(predicate);
        Check.NotNull(defaultResultFunc);

        var result = await TryGetFirstSuccessfulResultAsync(tasks, predicate).NoCapture();
        return result.HasResult ? result.Result : defaultResultFunc();
    }

    public static async Task<T> WhenAnySuccess<T>(this IEnumerable<Task<T>> tasks, Func<T, bool> predicate)
    {
        Check.NotNull(tasks);
        Check.NotNull(predicate);

        var result = await TryGetFirstSuccessfulResultAsync(tasks, predicate).NoCapture();
        return result.HasResult
            ? result.Result
            : throw new InvalidOperationException("No task produced an acceptable successful result.");
    }

    public static async Task WhenAnySuccess(this IEnumerable<Task> tasks)
    {
        Check.NotNull(tasks);

        var remainingTasks = tasks.ToList();
        while (remainingTasks.Count > 0)
        {
            var completedTask = await Task.WhenAny(remainingTasks).NoCapture();
            remainingTasks.Remove(completedTask);

            if (completedTask.Status == TaskStatus.RanToCompletion)
                return;

            if (completedTask.IsFaulted)
                _ = completedTask.Exception;
        }

        throw new InvalidOperationException("No task completed successfully.");
    }

    /// <summary>
    /// Completes when every task succeeds, or propagates the first observed fault or cancellation without waiting for the
    /// other tasks to finish.
    /// </summary>
    /// <remarks>
    /// This method cannot cancel the remaining tasks. If it returns early, it continues to observe their faults so they do
    /// not become unobserved task exceptions.
    /// </remarks>
    public static async Task WhenAllOrError(this IEnumerable<Task> tasks)
    {
        Check.NotNull(tasks);

        var remainingTasks = new List<Task>();
        foreach (var task in tasks)
        {
            if (task is null)
                throw new ArgumentException("The sequence cannot contain a null task.", nameof(tasks));

            ObserveFault(task);
            remainingTasks.Add(task);
        }

        while (remainingTasks.Count > 0)
        {
            var completedTask = await Task.WhenAny(remainingTasks).NoCapture();
            remainingTasks.Remove(completedTask);
            await completedTask.NoCapture();
        }

        static void ObserveFault(Task task)
        {
            _ = task.ContinueWith(
                static faultedTask => _ = faultedTask.Exception,
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnFaulted,
                TaskScheduler.Default);
        }
    }

}
