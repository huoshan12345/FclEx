using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FclEx;
using FclEx.Extensions;

namespace FclEx.Helpers;

public static class TaskHelper
{
    public static Task<TResult[]> Repeat<TResult>(Func<TResult> action, int times)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        if (times < 1) throw new ArgumentOutOfRangeException(nameof(times));

        var tasks = Enumerable.Repeat(Task.Run(action), times);
        return Task.WhenAll(tasks);
    }

    public static Task Repeat(Action action, int times)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        if (times < 1) throw new ArgumentOutOfRangeException(nameof(times));

        var tasks = Enumerable.Repeat(Task.Run(action), times);
        return Task.WhenAll(tasks);
    }

    public static Task<TResult[]> Repeat<TResult>(Func<Task<TResult>> action, int times)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        if (times < 1) throw new ArgumentOutOfRangeException(nameof(times));

        var tasks = Enumerable.Repeat(action, times).Select(m => m());
        return Task.WhenAll(tasks);
    }

    public static Task Repeat(Func<Task> action, int times)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        if (times < 1) throw new ArgumentOutOfRangeException(nameof(times));

        var tasks = Enumerable.Repeat(action, times).Select(m => m());
        return Task.WhenAll(tasks);
    }

    public static Task Delay(int seconds, CancellationToken token = default)
    {
        return Delay(TimeSpan.FromSeconds(seconds), token);
    }

    public static Task DelayMilli(int milliSeconds, CancellationToken token = default)
    {
        return Delay(TimeSpan.FromMilliseconds(milliSeconds), token);
    }

    public static async Task Delay(TimeSpan span, CancellationToken token = default)
    {
        if (span.Ticks <= 0)
            return;
        try
        {
            await Task.Delay(span, token).DonotCapture();
        }
        catch (TaskCanceledException) { }
    }

    public static Task<TResult> Run<TResult>(Func<Task<TResult>> task, TimeSpan? timeout = null)
    {
        return timeout is { } time
            ? Task.Run(task).WaitAsync(time)
            : task();
    }

    public static Task<TResult> Run<TResult>(Func<ValueTask<TResult>> task, TimeSpan? timeout = null)
    {
        return Run((Func<Task<TResult>>)(async () => await task()), timeout);
    }

    public static Task Run(Func<Task> task, TimeSpan? timeout = null)
    {
        return timeout is { } time
            ? Task.Run(task).WaitAsync(time)
            : task();
    }

    public static Task Run(Func<ValueTask> task, TimeSpan? timeout = null)
    {
        return Run((Func<Task>)(async () => await task()), timeout);
    }
}