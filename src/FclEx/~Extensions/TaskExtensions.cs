using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Helpers;
using FclEx.Utils;

namespace FclEx
{
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

        public static ValueTask<T> ToValueTask<T>(this Task<T> task) => new ValueTask<T>(task);

        public static void RunWithoutSyncContext(this Task task)
        {
            NoSyncContextScope.Run(task);
        }

        public static T RunWithoutSyncContext<T>(this Task<T> task)
        {
            return NoSyncContextScope.Run(task);
        }

        public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            using var cts = new CancellationTokenSource();
            var completedTask = await Task.WhenAny(task, TaskHelper.Delay(timeout, cts.Token));
            if (completedTask == task)
            {
                cts.Cancel();
                return await task.DonotCapture(); 
            }
            else
            {
                throw new TimeoutException("The operation has timed out.");
            }
        }

        public static async Task TimeoutAfter(this Task task, TimeSpan timeout)
        {
            using var cts = new CancellationTokenSource();
            var completedTask = await Task.WhenAny(task, TaskHelper.Delay(timeout, cts.Token));
            if (completedTask == task)
            {
                cts.Cancel();
                await task.DonotCapture();
            }
            else
            {
                throw new TimeoutException("The operation has timed out.");
            }
        }
    }
}
