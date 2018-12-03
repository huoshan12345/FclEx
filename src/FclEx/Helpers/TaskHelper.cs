using System;
using System.Collections.Generic;
using System.Linq;
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

        public static Task Delay(int seconds)
        {
            return seconds > 0 
                ? Task.Delay(seconds * 1000) 
                : Task.CompletedTask;
        }

        public static Task DelayMilli(int milliSeconds)
        {
            return milliSeconds > 0
                ? Task.Delay(milliSeconds)
                : Task.CompletedTask;
        }
    }
}
