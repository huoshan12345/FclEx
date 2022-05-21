using System;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Extensions;
using FclEx.Helpers;


namespace FclEx.Utils;

partial class Operate
{
    public static async Task<OperateResult> ExcuteAsync(Func<Task> action)
    {
        var watch = ValueStopwatch.StartNew();
        try
        {
            await action().DonotCapture();
            return new(default, watch.GetElapsedTime());
        }
        catch (Exception ex)
        {
            return (ex, watch.GetElapsedTime());
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
            await TaskHelper.TimeoutAfter(action, timeout).DonotCapture();
            return CreateSuccess(watch.GetElapsedTime());
        }
        catch (Exception ex)
        {
            return (ex, watch.GetElapsedTime());
        }
    }

    public static async Task<OperateResult<T>> ExcuteAsync<T>(Func<Task<T>> action)
    {
        var watch = ValueStopwatch.StartNew();
        try
        {
            var result = await action().DonotCapture();
            return (result, watch.GetElapsedTime());
        }
        catch (Exception ex)
        {
            return (ex, watch.GetElapsedTime());
        }
    }

    public static async Task<OperateResult<T>> ExcuteAsync<T>(Func<Task<T>> action, TimeSpan timeout)
    {
        var watch = ValueStopwatch.StartNew();
        try
        {
            var result = await TaskHelper.TimeoutAfter(action, timeout).DonotCapture();
            return (result, watch.GetElapsedTime());
        }
        catch (Exception ex)
        {
            return (ex, watch.GetElapsedTime());
        }
    }

    public static async Task<OperateResult> ExcuteAsync(Func<Task<OperateResult>> action)
        => (await ExcuteAsync<OperateResult>(action).DonotCapture()).Unwrap();

    public static async Task<OperateResult> ExcuteAsync(Func<Task<OperateResult>> action, TimeSpan timeout)
        => (await ExcuteAsync<OperateResult>(action, timeout).DonotCapture()).Unwrap();

    public static async Task<OperateResult<T>> ExcuteAsync<T>(Func<Task<OperateResult<T>>> action)
        => (await ExcuteAsync<OperateResult<T>>(action).DonotCapture()).Unwrap();

    public static async Task<OperateResult<T>> ExcuteAsync<T>(Func<Task<OperateResult<T>>> action, TimeSpan timeout)
        => (await ExcuteAsync<OperateResult<T>>(action, timeout).DonotCapture()).Unwrap();

    public static async ValueTask<OperateResult<T>> ExcuteValueAsync<T>(Func<ValueTask<T>> action)
    {
        var watch = ValueStopwatch.StartNew();
        try
        {
            var result = await action().DonotCapture();
            return (result, watch.GetElapsedTime());
        }
        catch (Exception ex)
        {
            return (ex, watch.GetElapsedTime());
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
            return (ex, watch.GetElapsedTime());
        }
    }

    public static async ValueTask<OperateResult<T>> ExcuteValueAsync<T>(Func<ValueTask<OperateResult<T>>> action)
        => (await ExcuteValueAsync<OperateResult<T>>(action)).Unwrap();
}