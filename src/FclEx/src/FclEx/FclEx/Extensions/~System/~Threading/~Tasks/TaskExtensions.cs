using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

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

    public static Task<T> On<T>(this Task<T> task, Func<T, bool> condition, Action<T> action)
    {
        return task.ContinueWith(t =>
        {
            if (condition(t.Result))
                action(t.Result);
            return t.Result;
        }, TaskContinuationOptions.OnlyOnRanToCompletion);
    }

    public static Task<T> On<T>(this Task<T> task, Func<T, bool> condition, Func<T, Task> action)
    {
        return task.ContinueWith(t =>
        {
            if (condition(t.Result))
                action(t.Result);
            return t.Result;
        }, TaskContinuationOptions.OnlyOnRanToCompletion);
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