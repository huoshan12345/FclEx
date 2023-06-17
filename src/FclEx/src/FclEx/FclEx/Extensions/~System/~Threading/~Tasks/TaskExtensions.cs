namespace FclEx.Extensions;

public static class TaskExtensions
{
    public static Task WhenAll(this IEnumerable<Task> tasks)
    {
        return Task.WhenAll(tasks);
    }

    public static Task<T[]> WhenAll<T>(this IEnumerable<Task<T>> tasks)
    {
        return Task.WhenAll(tasks);
    }

    public static ConfiguredTaskAwaitable DonotCapture(this Task task)
    {
        return task.ConfigureAwait(false);
    }

    public static ConfiguredTaskAwaitable<T> DonotCapture<T>(this Task<T> task)
    {
        return task.ConfigureAwait(false);
    }

    public static Task<T> ToTask<T>(this T obj) => Task.FromResult(obj);

    public static ValueTask<T> ToValueTask<T>(this Task<T> task) => new(task);

    public static async Task<T> On<T>(this Task<T> task, Func<T, bool> condition, Action<T> action)
    {
        var result = await NoSyncContextScope.RunAsync(() => task);
        if (condition(result))
            action(result);
        return result;
    }

    public static async Task<T> On<T>(this Task<T> task, Func<T, bool> condition, Func<T, Task> action)
    {
        var result = await NoSyncContextScope.RunAsync(() => task);
        if (condition(result))
            await action(result);
        return result;
    }

    [SuppressMessage("ReSharper", "MethodSupportsCancellation")]
    public static Task<TNewResult> ContinueWith<T, TNewResult>(this Task<T> task, Func<Task<T>, Task<TNewResult>> continuationFunction, CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<TNewResult>();
        task.ContinueWith(t =>
        {
            if (cancellationToken.IsCancellationRequested)
            {
                tcs.SetCanceled();
            }
            continuationFunction(t).ContinueWith(t2 =>
            {
                if (cancellationToken.IsCancellationRequested || t2.IsCanceled)
                {
                    tcs.TrySetCanceled();
                }
                else if (t2.IsFaulted)
                {
                    tcs.TrySetException(t2.Exception!);
                }
                else
                {
                    tcs.TrySetResult(t2.Result);
                }
            });
        });
        return tcs.Task;
    }
}