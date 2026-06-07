namespace FclEx.Utils;

partial class Operation
{
    public static Task<OperationResult> ExecuteAsync(Action action, TimeSpan? timeout = null)
    {
        Check.NotNull(action);
        return ExecuteAsync(() => Task.Run(action), timeout);
    }

    public static Task<OperationResult> ExecuteAsync(Func<OperationResult> action, TimeSpan? timeout = null)
    {
        Check.NotNull(action);
        return ExecuteAsync(() => Task.Run(action), timeout);
    }

    public static async Task<OperationResult> ExecuteAsync(Func<Task> action, TimeSpan? timeout = null)
    {
        Check.NotNull(action);

        var watch = ValueStopwatch.StartNew();
        try
        {
            await TaskHelper.Run(action, timeout).NoCapture();
            return Success(watch.GetElapsedTime());
        }
        catch (Exception ex)
        {
            return (ex, watch.GetElapsedTime());
        }
    }

    public static Task<OperationResult> ExecuteAsync(Func<Task<OperationResult>> action, TimeSpan? timeout = null)
    {
        Check.NotNull(action);
        return ExecuteAsync<OperationResult>(action, timeout).Then(m => m.Flatten());
    }

    public static Task<OperationResult<T>> ExecuteAsync<T>(Func<T> action, TimeSpan? timeout = null)
    {
        Check.NotNull(action);
        return ExecuteAsync(() => Task.Run(action), timeout);
    }

    public static Task<OperationResult<T>> ExecuteAsync<T>(Func<OperationResult<T>> action, TimeSpan? timeout = null)
    {
        Check.NotNull(action);
        return ExecuteAsync(() => Task.Run(action), timeout);
    }

    public static Task<OperationResult<T>> ExecuteAsync<T>(Func<Task<OperationResult<T>>> action, TimeSpan? timeout = null)
    {
        Check.NotNull(action);
        return ExecuteAsync<OperationResult<T>>(action, timeout).Then(m => m.Flatten());
    }

    public static async Task<OperationResult<T>> ExecuteAsync<T>(Func<Task<T>> action, TimeSpan? timeout = null)
    {
        Check.NotNull(action);

        var watch = ValueStopwatch.StartNew();
        try
        {
            var result = await TaskHelper.Run(action, timeout).NoCapture();
            return (result, watch.GetElapsedTime());
        }
        catch (Exception ex)
        {
            return (ex, watch.GetElapsedTime());
        }
    }

    public static async ValueTask<OperationResult<T>> ExecuteValueAsync<T>(Func<ValueTask<T>> action, TimeSpan? timeout = null)
    {
        Check.NotNull(action);

        var watch = ValueStopwatch.StartNew();
        try
        {
            var result = await TaskHelper.Run(action, timeout).NoCapture();
            return (result, watch.GetElapsedTime());
        }
        catch (Exception ex)
        {
            return (ex, watch.GetElapsedTime());
        }
    }

    public static async ValueTask<OperationResult> ExecuteValueAsync(Func<ValueTask<OperationResult>> action, TimeSpan? timeout = null)
    {
        Check.NotNull(action);
        return (await ExecuteValueAsync<OperationResult>(action, timeout).NoCapture()).Flatten();
    }

    public static async ValueTask<OperationResult> ExecuteValueAsync(Func<ValueTask> action, TimeSpan? timeout = null)
    {
        Check.NotNull(action);

        var watch = ValueStopwatch.StartNew();
        try
        {
            await TaskHelper.Run(action, timeout).NoCapture();
            return Success(watch.GetElapsedTime());
        }
        catch (Exception ex)
        {
            return (ex, watch.GetElapsedTime());
        }
    }

    public static async ValueTask<OperationResult<T>> ExecuteValueAsync<T>(Func<ValueTask<OperationResult<T>>> action, TimeSpan? timeout = null)
    {
        Check.NotNull(action);
        return (await ExecuteValueAsync<OperationResult<T>>(action, timeout).NoCapture()).Flatten();
    }
}
