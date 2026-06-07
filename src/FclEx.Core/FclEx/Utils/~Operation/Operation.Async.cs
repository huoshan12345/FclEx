namespace FclEx.Utils;

partial class Operation
{
    public static Task<OperationResult> ExecuteAsync(Action action, TimeSpan? timeout = null)
        => ExecuteAsync(() => Task.Run(action), timeout);

    public static Task<OperationResult> ExecuteAsync(Func<OperationResult> action, TimeSpan? timeout = null)
        => ExecuteAsync(() => Task.Run(action), timeout);

    public static async Task<OperationResult> ExecuteAsync(Func<Task> action, TimeSpan? timeout = null)
    {
        var watch = ValueStopwatch.StartNew();
        try
        {
            await TaskHelper.Run(action, timeout).ConfigureAwait(false);
            return Success(watch.GetElapsedTime());
        }
        catch (Exception ex)
        {
            return (ex, watch.GetElapsedTime());
        }
    }

    public static Task<OperationResult> ExecuteAsync(Func<Task<OperationResult>> action, TimeSpan? timeout = null)
        => ExecuteAsync<OperationResult>(action, timeout).Then(m => m.Unwrap());

    public static Task<OperationResult<T>> ExecuteAsync<T>(Func<T> action, TimeSpan? timeout = null)
        => ExecuteAsync(() => Task.Run(action), timeout);

    public static Task<OperationResult<T>> ExecuteAsync<T>(Func<OperationResult<T>> action, TimeSpan? timeout = null)
        => ExecuteAsync(() => Task.Run(action), timeout);

    public static Task<OperationResult<T>> ExecuteAsync<T>(Func<Task<OperationResult<T>>> action, TimeSpan? timeout = null)
       => ExecuteAsync<OperationResult<T>>(action, timeout).Then(m => m.Unwrap());

    public static async Task<OperationResult<T>> ExecuteAsync<T>(Func<Task<T>> action, TimeSpan? timeout = null)
    {
        var watch = ValueStopwatch.StartNew();
        try
        {
            var result = await TaskHelper.Run(action, timeout).ConfigureAwait(false);
            return (result, watch.GetElapsedTime());
        }
        catch (Exception ex)
        {
            return (ex, watch.GetElapsedTime());
        }
    }

    public static async ValueTask<OperationResult<T>> ExecuteValueAsync<T>(Func<ValueTask<T>> action, TimeSpan? timeout = null)
    {
        var watch = ValueStopwatch.StartNew();
        try
        {
            var result = await TaskHelper.Run(action, timeout).ConfigureAwait(false);
            return (result, watch.GetElapsedTime());
        }
        catch (Exception ex)
        {
            return (ex, watch.GetElapsedTime());
        }
    }

    public static async ValueTask<OperationResult> ExecuteValueAsync(Func<ValueTask<OperationResult>> action, TimeSpan? timeout = null)
        => (await ExecuteValueAsync<OperationResult>(action, timeout).ConfigureAwait(false)).Unwrap();

    public static async ValueTask<OperationResult> ExecuteValueAsync(Func<ValueTask> action, TimeSpan? timeout = null)
    {
        var watch = ValueStopwatch.StartNew();
        try
        {
            await TaskHelper.Run(action, timeout).ConfigureAwait(false);
            return Success(watch.GetElapsedTime());
        }
        catch (Exception ex)
        {
            return (ex, watch.GetElapsedTime());
        }
    }

    public static async ValueTask<OperationResult<T>> ExecuteValueAsync<T>(Func<ValueTask<OperationResult<T>>> action, TimeSpan? timeout = null)
        => (await ExecuteValueAsync<OperationResult<T>>(action, timeout).ConfigureAwait(false)).Unwrap();
}