using System.Threading.Tasks.Dataflow;

namespace FclEx.Extensions;

partial class EnumerableExtensions
{
    public static Task WhenAll(this IEnumerable<Task> tasks) => Task.WhenAll(tasks);

    public static Task<T[]> WhenAll<T>(this IEnumerable<Task<T>> tasks) => Task.WhenAll(tasks);

    public static async Task ToSeriallyExecutedTask<T>(this IEnumerable<T> enumerable, Func<T, Task> taskSelector, int intervalSeconds = 0, CancellationToken token = default)
    {
        Check.NotNull(enumerable);
        Check.NotNull(taskSelector);

        foreach (var item in enumerable)
        {
            if (token.IsCancellationRequested)
                break;

            await taskSelector(item);
            await TaskHelper.Delay(intervalSeconds, token);
        }
    }

    public static async Task<List<TResult>> ToSeriallyExecutedTask<T, TResult>(this IEnumerable<T> enumerable,
        Func<T, Task<TResult>> taskSelector, int intervalSeconds = 0, CancellationToken token = default)
    {
        // ReSharper disable once PossibleMultipleEnumeration
        Check.NotNull(enumerable);
        Check.NotNull(taskSelector);

        var list = new List<TResult>();
        // ReSharper disable once PossibleMultipleEnumeration
        foreach (var item in enumerable)
        {
            if (token.IsCancellationRequested)
                break;

            var r = await taskSelector(item);
            list.Add(r);
            await TaskHelper.Delay(intervalSeconds, token);
        }
        return list;
    }

    public static async Task<OperateResult<List<T>>> ToSeriallyExecutedTask<T>(this IEnumerable<T> enumerable, Func<T, Task<OperateResult<T>>> taskSelector,
        int intervalSeconds = 0, CancellationToken token = default, bool terminateOnFirstError = false)
    {
        Check.NotNull(enumerable);
        Check.NotNull(taskSelector);
        var span = TimeSpan.Zero;
        var list = new List<T>();
        IList<Exception>? exceptions = null;
        foreach (var obj in enumerable)
        {
            if (!token.IsCancellationRequested)
            {
                var r = await taskSelector(obj).IgnoreSyncContext();
                span += r.Elapsed;
                if (r.Success)
                {
                    list.Add(r.Value!);
                }
                else
                {
                    if (terminateOnFirstError)
                    {
                        return r.ToExplicit<List<T>>();
                    }
                    else
                    {
                        exceptions ??= new List<Exception>();
                        exceptions.Add(r.Exception!);
                    }
                }
                await TaskHelper.Delay(intervalSeconds, token);

            }
            else
            {
                break;
            }
        }
        if (exceptions.IsNotEmpty())
        {
            return (new AggregateException(exceptions), span);
        }
        else
        {
            return (list, span);
        }
    }

    public static async Task<List<TResult>> ToParallellyExecutedTask<T, TResult>(this IEnumerable<T> enumerable,
        Func<T, Task<TResult>> taskSelector, int batchSize, CancellationToken token = default)
    {
        // ReSharper disable once PossibleMultipleEnumeration
        Check.NotNull(enumerable);
        Check.NotNull(taskSelector);
        Check.NotLessThan(batchSize, 1);

        var list = new List<TResult>();
        // ReSharper disable once PossibleMultipleEnumeration
        foreach (var batch in enumerable.Chunk(batchSize))
        {
            if (token.IsCancellationRequested)
                break;

            var rs = await batch.Select(taskSelector).WhenAll();
            list.AddRange(rs);
        }
        return list;
    }

    public static async Task ToParallellyExecutedTask<T>(this IEnumerable<T> enumerable,
        Func<T, Task> taskSelector, int batchSize, CancellationToken token = default)
    {
        // ReSharper disable once PossibleMultipleEnumeration
        Check.NotNull(enumerable);
        Check.NotNull(taskSelector);
        Check.NotLessThan(batchSize, 1);

        // ReSharper disable once PossibleMultipleEnumeration
        foreach (var batch in enumerable.Chunk(batchSize))
        {
            if (token.IsCancellationRequested)
                break;
            await batch.Select(taskSelector).WhenAll();
        }
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
                if (t.IsCompletedSuccessfully && predicate(t.Result))
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
        var tcs = new TaskCompletionSource();
        var taskList = tasks.AsIReadOnlyList();
        var count = taskList.Count;
        var completedCount = 0;

        foreach (var task in taskList)
        {
            task.ContinueWith(t =>
            {
                if (t.IsCompletedSuccessfully)
                {
                    tcs.TrySetResult();
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

    public static Task ParallelForEachAsync<T>(this IEnumerable<T> source, ParallelOptions options, Func<T, CancellationToken, ValueTask> body)
    {
        return Parallel.ForEachAsync(source, options, body);
    }

    public static Task ParallelForEachAsync<T>(this IEnumerable<T> source, Func<T, CancellationToken, ValueTask> body, int maxDegreeOfParallelism = DataflowBlockOptions.Unbounded, CancellationToken token = default)
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
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(parallelOptions);
        ArgumentNullException.ThrowIfNull(body);

        List<TResult> results = [];
        if (source.TryGetNonEnumeratedCount(out var count))
            results.Capacity = count;

        IEnumerable<(TSource, int)> withIndexes = source.Select((x, i) => (x, i));

        return Parallel.ForEachAsync(withIndexes, parallelOptions, async (entry, ct) =>
        {
            var (item, index) = entry;
            var result = await body(item, ct).ConfigureAwait(false);
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
}