using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Helpers;
using MoreLinq;

namespace FclEx.Extensions;

partial class EnumerableExtensions
{
    public static Task<(List<(T Input, TResult Output)> Success, List<(T Input, OperateResult<TResult> Output)> Failure)>
        ToParallellyExecutedTaskOfPair<T, TResult>(this IEnumerable<T> enumerable, Func<T, Task<TResult>> taskSelector,
            int batchSize, CancellationToken token = default)
    {
        return enumerable.ToParallellyExecutedTaskOfPair(async m => Operate.CreateSuccess(await taskSelector(m)), batchSize, token);
    }


    public static async Task<(List<(T Input, TResult Output)> Success, List<(T Input, OperateResult<TResult> Output)> Failure)>
        ToParallellyExecutedTaskOfPair<T, TResult>(this IEnumerable<T> enumerable, Func<T, Task<OperateResult<TResult>>> taskSelector,
            int batchSize, CancellationToken token = default)
    {
        // ReSharper disable once PossibleMultipleEnumeration
        Check.NotNull(enumerable);
        Check.NotNull(taskSelector);
        Check.NotLessThan(batchSize, 1);

        var success = new List<(T, TResult)>();
        var failure = new List<(T, OperateResult<TResult>)>();
        // ReSharper disable once PossibleMultipleEnumeration
        foreach (var batch in enumerable.Batch(batchSize))
        {
            if (token.IsCancellationRequested)
            {
                failure.AddRange(batch.Select(m => (m, Operate.CreateCancel<TResult>())));
            }
            else
            {
                var rs = await batch.Select(async m => (m, await Operate.ExecuteAsync(() => taskSelector(m)))).WhenAll();
                foreach (var (i, o) in rs)
                {
                    if (o.Success)
                        success.Add((i, o.Value!));
                    else
                        failure.Add((i, o));
                }
            }
        }
        return (success, failure);
    }

    public static Task<(List<(T Input, TResult Output)> Success, List<(T Input, OperateResult<TResult> Output)> Failure)>
        ToSeriallyExecutedTaskOfPair<T, TResult>(this IEnumerable<T> enumerable, Func<T, Task<TResult>> taskSelector,
            int intervalSeconds = 0, CancellationToken token = default)
    {
        return enumerable.ToSeriallyExecutedTaskOfPair(async m => Operate.CreateSuccess(await taskSelector(m)), intervalSeconds, token);
    }

    public static async Task<(List<(T Input, TResult Output)> Success, List<(T Input, OperateResult<TResult> Output)> Failure)>
        ToSeriallyExecutedTaskOfPair<T, TResult>(this IEnumerable<T> enumerable, Func<T, Task<OperateResult<TResult>>> taskSelector,
            int intervalSeconds = 0, CancellationToken token = default)
    {
        // ReSharper disable once PossibleMultipleEnumeration
        Check.NotNull(enumerable);
        Check.NotNull(taskSelector);

        var success = new List<(T, TResult)>();
        var failure = new List<(T, OperateResult<TResult>)>();

        // ReSharper disable once PossibleMultipleEnumeration
        foreach (var item in enumerable)
        {
            if (token.IsCancellationRequested)
            {
                failure.Add((item, Operate.CreateCancel<TResult>()));
            }
            else
            {
                var r = await Operate.ExecuteAsync(() => taskSelector(item));
                if (r.Success)
                    success.Add((item, r.Value!));
                else
                    failure.Add((item, r));
            }
            await TaskHelper.Delay(intervalSeconds, token);
        }
        return (success, failure);
    }

    public static async Task ToSeriallyExecutedTask<T>(this IEnumerable<T> enumerable, Func<T, Task> taskSelector,
        int intervalSeconds = 0, CancellationToken token = default)
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

    public static async Task<List<TResult>> ToSeriallyExecutedTask<T, TResult>(this IEnumerable<T> enumerable, Func<T, Task<TResult>> taskSelector,
        int intervalSeconds = 0, CancellationToken token = default)
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

    public static async Task<OperateResult<List<T>>> ToSeriallyExecutedTask<T>(this IEnumerable<T> enumerable,
        Func<T, Task<OperateResult<T>>> taskSelector, int intervalSeconds = 0, CancellationToken token = default, bool terminateOnFirstError = false)
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
                var r = await taskSelector(obj).DonotCapture();
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
        if (exceptions.IsValid())
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
        foreach (var batch in enumerable.Batch(batchSize))
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
        foreach (var batch in enumerable.Batch(batchSize))
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
}