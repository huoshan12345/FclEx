using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

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
    }
}
