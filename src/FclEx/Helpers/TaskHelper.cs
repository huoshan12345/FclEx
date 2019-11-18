using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FclEx.Helpers
{
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
                await Task.Delay(span, token);
            }
            catch (TaskCanceledException) { }
        }

        /// <summary>
        /// Runs a TPL Task fire-and-forget style, the right way - in the
        /// background, separate from the current thread, with no risk
        /// of it trying to rejoin the current thread.
        /// </summary>
        public static void RunBg(Action action) => Task.Run(action).DonotCapture();
        public static void RunBg<T>(Func<T> action) => Task.Run(action).DonotCapture();
        public static void RunBg(Func<Task> fn) => Task.Run(fn).DonotCapture();
        public static void RunBg<T>(Func<Task<T>> fn) => Task.Run(fn).DonotCapture();
        public static void RunBg(Func<ValueTask> fn) => Task.Run(async () => await fn().DonotCapture()).DonotCapture();
        public static void RunBg<T>(Func<ValueTask<T>> fn) => Task.Run(async () => await fn().DonotCapture()).DonotCapture();
    }
}
