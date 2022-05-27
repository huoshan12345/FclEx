using System;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Extensions;
using FclEx.Helpers;


namespace FclEx.Utils;

partial class Operate
{
    public static Task<OperateResult> ExcuteAsync(Action action, TimeSpan? timeout = null)
        => ExcuteAsync(() => Task.Run(action), timeout);

    public static Task<OperateResult> ExcuteAsync(Func<OperateResult> action, TimeSpan? timeout = null)
        => ExcuteAsync(() => Task.Run(action), timeout);

    public static async Task<OperateResult> ExcuteAsync(Func<Task> action, TimeSpan? timeout = null)
    {
        var watch = ValueStopwatch.StartNew();
        try
        {
            await TaskHelper.Run(action, timeout).DonotCapture();
            return CreateSuccess(watch.GetElapsedTime());
        }
        catch (Exception ex)
        {
            return (ex, watch.GetElapsedTime());
        }
    }

    public static async Task<OperateResult> ExcuteAsync(Func<Task<OperateResult>> action, TimeSpan? timeout = null)
        => (await ExcuteAsync<OperateResult>(action, timeout).DonotCapture()).Unwrap();


    public static Task<OperateResult<T>> ExcuteAsync<T>(Func<T> action, TimeSpan? timeout = null)
        => ExcuteAsync(() => Task.Run(action), timeout);

    public static Task<OperateResult<T>> ExcuteAsync<T>(Func<OperateResult<T>> action, TimeSpan? timeout = null)
        => ExcuteAsync(() => Task.Run(action), timeout);

    public static async Task<OperateResult<T>> ExcuteAsync<T>(Func<Task<OperateResult<T>>> action, TimeSpan? timeout = null)
       => (await ExcuteAsync<OperateResult<T>>(action, timeout).DonotCapture()).Unwrap();

    public static async Task<OperateResult<T>> ExcuteAsync<T>(Func<Task<T>> action, TimeSpan? timeout = null)
    {
        var watch = ValueStopwatch.StartNew();
        try
        {
            var result = await TaskHelper.Run(action, timeout);
            return (result, watch.GetElapsedTime());
        }
        catch (Exception ex)
        {
            return (ex, watch.GetElapsedTime());
        }
    }


    public static async ValueTask<OperateResult<T>> ExcuteValueAsync<T>(Func<ValueTask<T>> action, TimeSpan? timeout = null)
    {
        var watch = ValueStopwatch.StartNew();
        try
        {
            var result = await TaskHelper.Run(action, timeout);
            return (result, watch.GetElapsedTime());
        }
        catch (Exception ex)
        {
            return (ex, watch.GetElapsedTime());
        }
    }

    public static async ValueTask<OperateResult> ExcuteValueAsync(Func<ValueTask<OperateResult>> action, TimeSpan? timeout = null)
        => (await ExcuteValueAsync<OperateResult>(action, timeout)).Unwrap();

    public static async ValueTask<OperateResult> ExcuteValueAsync(Func<ValueTask> action, TimeSpan? timeout = null)
    {
        var watch = ValueStopwatch.StartNew();
        try
        {
            await TaskHelper.Run(action, timeout);
            return CreateSuccess(watch.GetElapsedTime());
        }
        catch (Exception ex)
        {
            return (ex, watch.GetElapsedTime());
        }
    }

    public static async ValueTask<OperateResult<T>> ExcuteValueAsync<T>(Func<ValueTask<OperateResult<T>>> action, TimeSpan? timeout = null)
        => (await ExcuteValueAsync<OperateResult<T>>(action)).Unwrap();
}