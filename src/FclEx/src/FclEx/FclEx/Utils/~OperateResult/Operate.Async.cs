using System.Diagnostics;
using FclEx.Helpers;


namespace FclEx.Utils;

partial class Operate
{
    public static Task<OperateResult> ExecuteAsync(Action action, TimeSpan? timeout = null)
        => ExecuteAsync(() => Task.Run(action), timeout);

    public static Task<OperateResult> ExecuteAsync(Func<OperateResult> action, TimeSpan? timeout = null)
        => ExecuteAsync(() => Task.Run(action), timeout);

    public static async Task<OperateResult> ExecuteAsync(Func<Task> action, TimeSpan? timeout = null)
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

    public static async Task<OperateResult> ExecuteAsync(Func<Task<OperateResult>> action, TimeSpan? timeout = null)
        => (await ExecuteAsync<OperateResult>(action, timeout).DonotCapture()).Unwrap();


    public static Task<OperateResult<T>> ExecuteAsync<T>(Func<T> action, TimeSpan? timeout = null)
        => ExecuteAsync(() => Task.Run(action), timeout);

    public static Task<OperateResult<T>> ExecuteAsync<T>(Func<OperateResult<T>> action, TimeSpan? timeout = null)
        => ExecuteAsync(() => Task.Run(action), timeout);

    public static async Task<OperateResult<T>> ExecuteAsync<T>(Func<Task<OperateResult<T>>> action, TimeSpan? timeout = null)
       => (await ExecuteAsync<OperateResult<T>>(action, timeout).DonotCapture()).Unwrap();

    public static async Task<OperateResult<T>> ExecuteAsync<T>(Func<Task<T>> action, TimeSpan? timeout = null)
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

    public static async ValueTask<OperateResult<T>> ExecuteValueAsync<T>(Func<ValueTask<T>> action, TimeSpan? timeout = null)
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

    public static async ValueTask<OperateResult> ExecuteValueAsync(Func<ValueTask<OperateResult>> action, TimeSpan? timeout = null)
        => (await ExecuteValueAsync<OperateResult>(action, timeout)).Unwrap();

    public static async ValueTask<OperateResult> ExecuteValueAsync(Func<ValueTask> action, TimeSpan? timeout = null)
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

    public static async ValueTask<OperateResult<T>> ExecuteValueAsync<T>(Func<ValueTask<OperateResult<T>>> action, TimeSpan? timeout = null)
        => (await ExecuteValueAsync<OperateResult<T>>(action)).Unwrap();
}