using System;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Helpers;


namespace FclEx.Utils
{
    public partial struct OperateResult
    {
        public static async Task<OperateResult> ExcuteAsync(Func<Task> action)
        {
            var watch = ValueStopwatch.StartNew();
            try
            {
                await action().DonotCapture();
                return CreateSuccess(watch.GetElapsedTime());
            }
            catch (Exception ex)
            {
                return CreateError(ex, watch.GetElapsedTime());
            }
        }

        public static Task<OperateResult> ExcuteAsync(Action action, TimeSpan timeout)
            => ExcuteAsync(() => Task.Run(action), timeout);

        public static Task<OperateResult> ExcuteAsync(Func<OperateResult> action, TimeSpan timeout)
            => ExcuteAsync(() => Task.Run(action), timeout);

        public static Task<OperateResult<T>> ExcuteAsync<T>(Func<T> action, TimeSpan timeout)
            => ExcuteAsync(() => Task.Run(action), timeout);

        public static Task<OperateResult<T>> ExcuteAsync<T>(Func<OperateResult<T>> action, TimeSpan timeout)
            => ExcuteAsync(() => Task.Run(action), timeout);

        public static async Task<OperateResult> ExcuteAsync(Func<Task> action, TimeSpan timeout)
        {
            var watch = ValueStopwatch.StartNew();
            try
            {
                await action().TimeoutAfter(timeout).DonotCapture();
                return CreateSuccess(watch.GetElapsedTime());
            }
            catch (Exception ex)
            {
                return CreateError(ex, watch.GetElapsedTime());
            }
        }

        public static async Task<OperateResult<T>> ExcuteAsync<T>(Func<Task<T>> action)
        {
            var watch = ValueStopwatch.StartNew();
            try
            {
                var result = await action().DonotCapture();
                return CreateSuccess(result, watch.GetElapsedTime());
            }
            catch (Exception ex)
            {
                return CreateError<T>(ex, watch.GetElapsedTime());
            }
        }

        public static async Task<OperateResult<T>> ExcuteAsync<T>(Func<Task<T>> action, TimeSpan timeout)
        {
            var watch = ValueStopwatch.StartNew();
            try
            {
                var result = await action().TimeoutAfter(timeout).DonotCapture();
                return CreateSuccess(result, watch.GetElapsedTime());
            }
            catch (Exception ex)
            {
                return CreateError(ex, watch.GetElapsedTime());
            }
        }

        public static async Task<OperateResult> ExcuteAsync(Func<Task<OperateResult>> action)
            => (await ExcuteAsync<OperateResult>(action)).Unwrap();

        public static async Task<OperateResult> ExcuteAsync(Func<Task<OperateResult>> action, TimeSpan timeout)
            => (await ExcuteAsync<OperateResult>(action, timeout)).Unwrap();

        public static async Task<OperateResult<T>> ExcuteAsync<T>(Func<Task<OperateResult<T>>> action)
            => (await ExcuteAsync<OperateResult<T>>(action)).Unwrap();

        public static async Task<OperateResult<T>> ExcuteAsync<T>(Func<Task<OperateResult<T>>> action, TimeSpan timeout)
            => (await ExcuteAsync<OperateResult<T>>(action, timeout)).Unwrap();

        public static async ValueTask<OperateResult<T>> ExcuteValueAsync<T>(Func<ValueTask<T>> action)
        {
            var watch = ValueStopwatch.StartNew();
            try
            {
                var result = await action().DonotCapture();
                return CreateSuccess(result, watch.GetElapsedTime());
            }
            catch (Exception ex)
            {
                return CreateError<T>(ex, watch.GetElapsedTime());
            }
        }

        public static async ValueTask<OperateResult> ExcuteValueAsync(Func<ValueTask<OperateResult>> action)
            => (await ExcuteValueAsync<OperateResult>(action)).Unwrap();

        public static async ValueTask<OperateResult> ExcuteValueAsync(Func<ValueTask> action)
        {
            var watch = ValueStopwatch.StartNew();
            try
            {
                await action().DonotCapture();
                return CreateSuccess(watch.GetElapsedTime());
            }
            catch (Exception ex)
            {
                return CreateError(ex, watch.GetElapsedTime());
            }
        }

        public static async ValueTask<OperateResult<T>> ExcuteValueAsync<T>(Func<ValueTask<OperateResult<T>>> action)
            => (await ExcuteValueAsync<OperateResult<T>>(action)).Unwrap();

        public static void ExcuteBgAsync(Func<Task> action) => TaskHelper.RunBg(() => ExcuteAsync(action));
        public static void ExcuteBgAsync<T>(Func<Task<T>> action) => TaskHelper.RunBg(() => ExcuteAsync(action));
        public static void ExcuteBgAsync(Func<Task<OperateResult>> action) => TaskHelper.RunBg(() => ExcuteAsync(action));
        public static void ExcuteBgAsync<T>(Func<Task<OperateResult<T>>> action) => TaskHelper.RunBg(() => ExcuteAsync(action));

        public static void ExcuteBgValueAsync(Func<ValueTask> action) => TaskHelper.RunBg(() => ExcuteValueAsync(action));
        public static void ExcuteBgValueAsync<T>(Func<ValueTask<T>> action) => TaskHelper.RunBg(() => ExcuteValueAsync(action));
        public static void ExcuteBgValueAsync(Func<ValueTask<OperateResult>> action) => TaskHelper.RunBg(() => ExcuteValueAsync(action));
        public static void ExcuteBgValueAsync<T>(Func<ValueTask<OperateResult<T>>> action) => TaskHelper.RunBg(() => ExcuteValueAsync(action));
    }
}
