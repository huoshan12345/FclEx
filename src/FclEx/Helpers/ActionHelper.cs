using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace FclEx.Helpers
{
    public static class ActionHelper
    {
        internal static Action<Exception> EmptyExpAction { get; } = e => { };

        public static void Try(Action action, int retryTimes = 3, int delaySeconds = 0,
            Action<Exception>? onFail = null, bool throwOnFail = false)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            var lastEx = default(Exception);
            onFail ??= EmptyExpAction;
            var times = Math.Max(0, retryTimes) + 1;
            for (var i = 1; i <= times; i++)
            {
                try
                {
                    action();
                    return;
                }
                catch (Exception ex)
                {
                    lastEx = ex;
                    ThreadHelper.Sleep(delaySeconds);
                }
            }

            if (lastEx == null) return;

            onFail(lastEx);
            if (throwOnFail) throw lastEx;
        }

        [return: MaybeNull]
        public static T Try<T>(Func<T> action, int retryTimes = 3, int delaySeconds = 0,
            Func<Exception, T>? onFail = null, bool throwOnFail = false)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            var times = Math.Max(0, retryTimes) + 1;
            var lastEx = default(Exception);
            for (var i = 1; i <= times; i++)
            {
                try
                {
                    return action();
                }
                catch (Exception ex)
                {
                    lastEx = ex;
                    ThreadHelper.Sleep(delaySeconds);
                }
            }
            if (throwOnFail && lastEx != null) throw lastEx;
            return (onFail == null || lastEx == null) ? default : onFail(lastEx);
        }

        public static async Task TryAsync(Func<Task> func, int retryTimes = 3, int delaySeconds = 0,
            Action<Exception>? onFail = null, bool throwOnFail = false)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));

            onFail ??= EmptyExpAction;
            var times = Math.Max(0, retryTimes) + 1;
            var lastEx = default(Exception);
            for (var i = 1; i <= times; i++)
            {
                try
                {
                    await func().DonotCapture();
                    return;
                }
                catch (Exception ex)
                {
                    lastEx = ex;
                    await TaskHelper.Delay(delaySeconds);
                }
            }

            if (lastEx == null)
                return;

            onFail(lastEx);

            if (throwOnFail)
                throw lastEx;
        }

        public static async Task<T> TryAsync<T>(Func<Task<T>> func, int retryTimes = 3, int delaySeconds = 0,
            Func<Exception, T>? onFail = null, bool throwOnFail = false, T defaultValue = default)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));
            var lastEx = default(Exception);
            var times = Math.Max(0, retryTimes) + 1;
            for (var i = 1; i <= times; i++)
            {
                try
                {
                    return await func().DonotCapture();
                }
                catch (Exception ex)
                {
                    lastEx = ex;
                    await TaskHelper.Delay(delaySeconds);
                }
            }

            if (throwOnFail && lastEx != null) throw lastEx;
            // waiting for https://github.com/dotnet/csharplang/issues/2946
#pragma warning disable CS8603 // Possible null reference return.
            return onFail == null || lastEx == null ? default : onFail(lastEx);
#pragma warning restore CS8603 // Possible null reference return.
        }
    }
}
