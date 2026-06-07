namespace FclEx.Extensions;

partial class EnumerableExtensions
{
    public static Task WhenAll(this IEnumerable<Task> tasks) => Task.WhenAll(tasks);

    public static Task<T[]> WhenAll<T>(this IEnumerable<Task<T>> tasks) => Task.WhenAll(tasks);

    public static async Task ExecuteSequentiallyAsync<T>(this IEnumerable<T> enumerable, Func<T, Task> operation,
        TimeSpan interval = default, CancellationToken token = default)
    {
        Check.NotNull(enumerable);
        Check.NotNull(operation);

        foreach (var item in enumerable)
        {
            if (token.IsCancellationRequested)
                break;

            await operation(item);

            await TaskHelper.Delay(interval, token);
        }
    }

    public static async Task<TResult[]> ExecuteSequentiallyAsync<T, TResult>(this IEnumerable<T> enumerable, Func<T, Task<TResult>> operation,
        TimeSpan interval = default, CancellationToken token = default)
    {
        Check.NotNull(enumerable);
        Check.NotNull(operation);

        var list = new List<TResult>();
        foreach (var item in enumerable)
        {
            if (token.IsCancellationRequested)
                break;

            var r = await operation(item);
            list.Add(r);

            await TaskHelper.Delay(interval, token);
        }
        return list.ToArray();
    }

    public static async Task ExecuteInParallelAsync<T>(this IEnumerable<T> enumerable, Func<T, Task> operation,
        int? concurrency = null, TimeSpan interval = default, CancellationToken token = default)
    {
        Check.NotNull(enumerable);
        Check.NotNull(operation);

        if (concurrency is null)
        {
            await enumerable.Select(operation).WhenAll();
            return;
        }

        var size = Check.Positive(concurrency.Value, nameof(concurrency));

        foreach (var batch in enumerable.Chunk(size))
        {
            if (token.IsCancellationRequested)
                break;

            await batch.Select(operation).WhenAll();

            await TaskHelper.Delay(interval, token);
        }
    }

    public static async Task<TResult[]> ExecuteInParallelAsync<T, TResult>(this IEnumerable<T> enumerable, Func<T, Task<TResult>> operation,
        int? concurrency = null, TimeSpan interval = default, CancellationToken token = default)
    {
        Check.NotNull(enumerable);
        Check.NotNull(operation);

        if (concurrency is null)
        {
            return await enumerable.Select(operation).WhenAll();
        }

        var size = Check.Positive(concurrency.Value, nameof(concurrency));

        var list = new List<TResult>();
        foreach (var batch in enumerable.Chunk(size))
        {
            if (token.IsCancellationRequested)
                break;

            var rs = await batch.Select(operation).WhenAll();
            list.AddRange(rs);

            await TaskHelper.Delay(interval, token);
        }
        return list.ToArray();
    }

    public static Task ExecuteAsync<T>(this IEnumerable<T> enumerable, Func<T, Task> operation, bool executeInParallel,
        int? concurrency = null, TimeSpan interval = default, CancellationToken token = default)
    {
        return executeInParallel
            ? enumerable.ExecuteInParallelAsync(operation, concurrency, interval, token)
            : enumerable.ExecuteSequentiallyAsync(operation, interval, token);
    }

    public static Task<TResult[]> ExecuteAsync<T, TResult>(this IEnumerable<T> enumerable, Func<T, Task<TResult>> operation, bool executeInParallel,
        int? concurrency = null, TimeSpan interval = default, CancellationToken token = default)
    {
        return executeInParallel
            ? enumerable.ExecuteInParallelAsync(operation, concurrency, interval, token)
            : enumerable.ExecuteSequentiallyAsync(operation, interval, token);
    }

    public static async Task<T> WhenAny<T>(this IEnumerable<Task<T>> tasks)
    {
        return await (await Task.WhenAny(tasks));
    }

    private static Task<T> WhenAnySuccess<T>(this IEnumerable<Task<T>> tasks, Func<T, bool> predicate, Action<TaskCompletionSource<T>> onNoResult)
    {
        var tcs = new TaskCompletionSource<T>();
        var taskList = tasks.AsIReadOnlyList();
        var count = taskList.Count;
        var completedCount = 0;

        foreach (var task in taskList)
        {
            task.ContinueWith(t =>
            {
                if (t.Status == TaskStatus.RanToCompletion && predicate(t.Result))
                {
                    tcs.TrySetResult(t.Result);
                }

                if (Interlocked.Increment(ref completedCount) >= count)
                {
                    onNoResult(tcs);
                }
            });
        }

        return tcs.Task;
    }

    public static Task<T> WhenAnySuccess<T>(this IEnumerable<Task<T>> tasks, Func<T, bool> predicate, Func<T> defaultResultFunc)
    {
        return tasks.WhenAnySuccess(predicate, tcs => tcs.TrySetResult(defaultResultFunc()));
    }

    public static Task<T> WhenAnySuccess<T>(this IEnumerable<Task<T>> tasks, Func<T, bool> predicate)
    {
        return tasks.WhenAnySuccess(predicate, tcs => tcs.SetException(new InvalidOperationException("All tasks failed")));
    }

    public static Task WhenAnySuccess(this IEnumerable<Task> tasks)
    {
        var tcs = new TaskCompletionSource<int>();
        var taskList = tasks.AsIReadOnlyList();
        var count = taskList.Count;
        var completedCount = 0;

        foreach (var task in taskList)
        {
            task.ContinueWith(t =>
            {
                if (t.Status == TaskStatus.RanToCompletion)
                {
                    tcs.TrySetResult(default);
                }

                if (Interlocked.Increment(ref completedCount) >= count)
                {
                    tcs.SetException(new InvalidOperationException("All tasks failed"));
                }
            });
        }

        return tcs.Task;
    }

    public static Task WhenAllOrError(this IEnumerable<Task> tasks)
    {
        var tcs = new TaskCompletionSource<int>();
        var taskList = tasks.AsIReadOnlyList();
        var completedCount = 0;

        foreach (var task in taskList)
        {
            task.ContinueWith(t =>
            {
                if (t.IsCanceled)
                {
                    tcs.TrySetCanceled();
                }
                else if (t.IsFaulted)
                {
                    tcs.TrySetException(t.Exception!.InnerExceptions);
                }
                else
                {
                    if (Interlocked.Increment(ref completedCount) == taskList.Count)
                    {
                        tcs.TrySetResult(0);
                    }
                }
            });
        }

        return tcs.Task;
    }

#if NET6_0_OR_GREATER
    public static Task ParallelForEachAsync<T>(this IEnumerable<T> source, ParallelOptions options, Func<T, CancellationToken, ValueTask> body)
    {
        return Parallel.ForEachAsync(source, options, body);
    }

    public static Task ParallelForEachAsync<T>(this IEnumerable<T> source, Func<T, CancellationToken, ValueTask> body, int maxDegreeOfParallelism = System.Threading.Tasks.Dataflow.DataflowBlockOptions.Unbounded, CancellationToken token = default)
    {
        return source.ParallelForEachAsync(new()
        {
            CancellationToken = token,
            MaxDegreeOfParallelism = maxDegreeOfParallelism,
            TaskScheduler = null,
        }, body);
    }
    /// <summary>
    /// Executes a foreach loop on an enumerable sequence, in which iterations may run
    /// in parallel, and returns the results of all iterations in the original order.
    /// </summary>
    public static Task<TResult[]> ForEachAsync<TSource, TResult>(IEnumerable<TSource> source, ParallelOptions parallelOptions, Func<TSource, CancellationToken, ValueTask<TResult>> body)
    {
        Check.NotNull(source);
        Check.NotNull(parallelOptions);
        Check.NotNull(body);

        List<TResult> results = [];

        if (source.TryGetNonEnumeratedCount(out var count))
            results.Capacity = count;

        IEnumerable<(TSource, int)> withIndexes = source.Select((x, i) => (x, i));

        return Parallel.ForEachAsync(withIndexes, parallelOptions, async (entry, ct) =>
        {
            var (item, index) = entry;
            var result = await body(item, ct).NoCapture();
            lock (results)
            {
#if NET8_0_OR_GREATER
                if (index >= results.Count)
                    CollectionsMarshal.SetCount(results, index + 1);
                results[index] = result;
#else
                results.Add(result);
#endif

            }
        }).ContinueWith(t =>
        {
            TaskCompletionSource<TResult[]> tcs = new();
            switch (t.Status)
            {
                case TaskStatus.RanToCompletion:
                    lock (results)
                    {
                        tcs.SetResult(results.ToArray());
                    }
                    break;
                case TaskStatus.Faulted:
                    tcs.SetException(t.Exception!.InnerExceptions);
                    break;
                case TaskStatus.Canceled:
                    tcs.SetCanceled(new TaskCanceledException(t).CancellationToken);
                    break;
                default:
                    throw new UnreachableException();
            }
            Debug.Assert(tcs.Task.IsCompleted);
            return tcs.Task;
        }, default, TaskContinuationOptions.DenyChildAttach |
                        TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default).Unwrap();
    }
#endif
}