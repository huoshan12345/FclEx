using System;
using System.Threading.Tasks;
using FclEx.Helpers;
using IOperateResult = FclEx.Utils.IOperateResult<FclEx.Utils.IUnit>;
using OperateResult = FclEx.Utils.OperateResult<FclEx.Utils.IUnit>;

namespace FclEx.Utils
{
    public static partial class OperateUtil
    {
        public static async Task<IOperateResult> ExcuteAsync(Func<Task> action)
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

        public static async Task<IOperateResult<T>> ExcuteAsync<T>(Func<Task<T>> action)
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

        public static async Task<IOperateResult> ExcuteAsync(Func<Task<IOperateResult>> action)
            => (await ExcuteAsync<IOperateResult>(action)).Unwrap();

        public static async Task<IOperateResult<T>> ExcuteAsync<T>(Func<Task<IOperateResult<T>>> action)
            => (await ExcuteAsync<IOperateResult<T>>(action)).Unwrap();

        public static async ValueTask<IOperateResult<T>> ExcuteValueAsync<T>(Func<ValueTask<T>> action)
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

        public static async ValueTask<IOperateResult> ExcuteValueAsync(Func<ValueTask<IOperateResult>> action)
            => (await ExcuteValueAsync<IOperateResult>(action)).Unwrap();

        public static async ValueTask<IOperateResult> ExcuteValueAsync(Func<ValueTask> action)
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

        public static async ValueTask<IOperateResult<T>> ExcuteValueAsync<T>(Func<ValueTask<IOperateResult<T>>> action)
            => (await ExcuteValueAsync<IOperateResult<T>>(action)).Unwrap();
        
        public static void ExcuteBgAsync(Func<Task> action) => TaskHelper.RunBg(() => ExcuteAsync(action));
        public static void ExcuteBgAsync<T>(Func<Task<T>> action) => TaskHelper.RunBg(() => ExcuteAsync(action));
        public static void ExcuteBgAsync(Func<Task<IOperateResult>> action) => TaskHelper.RunBg(() => ExcuteAsync(action));
        public static void ExcuteBgAsync<T>(Func<Task<IOperateResult<T>>> action) => TaskHelper.RunBg(() => ExcuteAsync(action));

        public static void ExcuteBgValueAsync(Func<ValueTask> action) => TaskHelper.RunBg(() => ExcuteValueAsync(action));
        public static void ExcuteBgValueAsync<T>(Func<ValueTask<T>> action) => TaskHelper.RunBg(() => ExcuteValueAsync(action));
        public static void ExcuteBgValueAsync(Func<ValueTask<IOperateResult>> action) => TaskHelper.RunBg(() => ExcuteValueAsync(action));
        public static void ExcuteBgValueAsync<T>(Func<ValueTask<IOperateResult<T>>> action) => TaskHelper.RunBg(() => ExcuteValueAsync(action));
    }
}
