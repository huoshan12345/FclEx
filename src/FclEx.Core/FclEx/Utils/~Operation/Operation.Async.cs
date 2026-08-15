namespace FclEx.Utils;

public static partial class Operation
{
    /// <summary>
    /// Runs a synchronous operation on the thread pool and converts its completion, exception, or timeout into an <see cref="OperationResult"/>.
    /// </summary>
    /// <param name="action">The operation to execute.</param>
    /// <param name="timeout">The optional maximum wait time.</param>
    /// <returns>A task that resolves to the operation result. Timeout returns an error result but does not cancel already-started synchronous work.</returns>
    public static Task<OperationResult> ExecuteAsync(Action action, TimeSpan? timeout = null)
    {
        Check.NotNull(action);
        return ExecuteAsync(() => Task.Run(action), timeout);
    }

    /// <summary>
    /// Runs a synchronous operation-result factory on the thread pool and flattens the nested result.
    /// </summary>
    /// <param name="action">The operation-result factory to execute.</param>
    /// <param name="timeout">The optional maximum wait time.</param>
    /// <returns>A task that resolves to the flattened result. Timeout returns an error result but does not cancel already-started synchronous work.</returns>
    public static Task<OperationResult> ExecuteAsync(Func<OperationResult> action, TimeSpan? timeout = null)
    {
        Check.NotNull(action);
        return ExecuteAsync(() => Task.Run(action), timeout);
    }

    /// <summary>
    /// Awaits an asynchronous operation and converts its completion, exception, cancellation, or timeout into an <see cref="OperationResult"/>.
    /// </summary>
    /// <param name="action">The asynchronous operation to execute.</param>
    /// <param name="timeout">The optional maximum wait time.</param>
    /// <returns>A task that resolves to the operation result. Timeout returns an error result and does not cancel the original task unless the task observes cancellation independently.</returns>
    public static async Task<OperationResult> ExecuteAsync(Func<Task> action, TimeSpan? timeout = null)
    {
        Check.NotNull(action);

        var watch = ValueStopwatch.StartNew();
        try
        {
            await TaskHelper.RunAsync(_ => action(), timeout).NoCapture();
            return Success(watch.GetElapsedTime());
        }
        catch (Exception ex)
        {
            return (ex, watch.GetElapsedTime());
        }
    }

    /// <summary>
    /// Awaits an asynchronous operation-result factory and flattens the nested result.
    /// </summary>
    /// <param name="action">The asynchronous operation-result factory to execute.</param>
    /// <param name="timeout">The optional maximum wait time.</param>
    /// <returns>A task that resolves to the flattened result. The outer execution elapsed time is used when flattening.</returns>
    public static Task<OperationResult> ExecuteAsync(Func<Task<OperationResult>> action, TimeSpan? timeout = null)
    {
        Check.NotNull(action);
        return ExecuteAsync<OperationResult>(action, timeout).Then(m => m.Flatten());
    }

    /// <summary>
    /// Runs a synchronous value-producing operation on the thread pool and converts its value, exception, or timeout into an <see cref="OperationResult{T}"/>.
    /// </summary>
    /// <typeparam name="T">The value type returned by the operation.</typeparam>
    /// <param name="action">The operation to execute.</param>
    /// <param name="timeout">The optional maximum wait time.</param>
    /// <returns>A task that resolves to the operation result. Timeout returns an error result but does not cancel already-started synchronous work.</returns>
    public static Task<OperationResult<T>> ExecuteAsync<T>(Func<T> action, TimeSpan? timeout = null)
    {
        Check.NotNull(action);
        return ExecuteAsync(() => Task.Run(action), timeout);
    }

    /// <summary>
    /// Runs a synchronous typed operation-result factory on the thread pool and flattens the nested result.
    /// </summary>
    /// <typeparam name="T">The value type returned by the inner result.</typeparam>
    /// <param name="action">The operation-result factory to execute.</param>
    /// <param name="timeout">The optional maximum wait time.</param>
    /// <returns>A task that resolves to the flattened result. Timeout returns an error result but does not cancel already-started synchronous work.</returns>
    public static Task<OperationResult<T>> ExecuteAsync<T>(Func<OperationResult<T>> action, TimeSpan? timeout = null)
    {
        Check.NotNull(action);
        return ExecuteAsync(() => Task.Run(action), timeout);
    }

    /// <summary>
    /// Awaits an asynchronous typed operation-result factory and flattens the nested result.
    /// </summary>
    /// <typeparam name="T">The value type returned by the inner result.</typeparam>
    /// <param name="action">The asynchronous operation-result factory to execute.</param>
    /// <param name="timeout">The optional maximum wait time.</param>
    /// <returns>A task that resolves to the flattened result. The outer execution elapsed time is used when flattening.</returns>
    public static Task<OperationResult<T>> ExecuteAsync<T>(Func<Task<OperationResult<T>>> action, TimeSpan? timeout = null)
    {
        Check.NotNull(action);
        return ExecuteAsync<OperationResult<T>>(action, timeout).Then(m => m.Flatten());
    }

    /// <summary>
    /// Awaits an asynchronous value-producing operation and converts its value, exception, cancellation, or timeout into an <see cref="OperationResult{T}"/>.
    /// </summary>
    /// <typeparam name="T">The value type returned by the operation.</typeparam>
    /// <param name="action">The asynchronous operation to execute.</param>
    /// <param name="timeout">The optional maximum wait time.</param>
    /// <returns>A task that resolves to the operation result. Timeout returns an error result and does not cancel the original task unless the task observes cancellation independently.</returns>
    public static async Task<OperationResult<T>> ExecuteAsync<T>(Func<Task<T>> action, TimeSpan? timeout = null)
    {
        Check.NotNull(action);

        var watch = ValueStopwatch.StartNew();
        try
        {
            var result = await TaskHelper.RunAsync(_ => action(), timeout).NoCapture();
            return (result, watch.GetElapsedTime());
        }
        catch (Exception ex)
        {
            return (ex, watch.GetElapsedTime());
        }
    }

    /// <summary>
    /// Awaits a <see cref="ValueTask{TResult}"/> operation and converts its value, exception, cancellation, or timeout into an <see cref="OperationResult{T}"/>.
    /// </summary>
    /// <typeparam name="T">The value type returned by the operation.</typeparam>
    /// <param name="action">The asynchronous operation to execute.</param>
    /// <param name="timeout">The optional maximum wait time.</param>
    /// <returns>A value task that resolves to the operation result.</returns>
    public static async ValueTask<OperationResult<T>> ExecuteValueAsync<T>(Func<ValueTask<T>> action, TimeSpan? timeout = null)
    {
        Check.NotNull(action);

        var watch = ValueStopwatch.StartNew();
        try
        {
            var result = await TaskHelper.RunValueTaskAsync(_ => action(), timeout).NoCapture();
            return (result, watch.GetElapsedTime());
        }
        catch (Exception ex)
        {
            return (ex, watch.GetElapsedTime());
        }
    }

    /// <summary>
    /// Awaits a <see cref="ValueTask{TResult}"/> operation-result factory and flattens the nested result.
    /// </summary>
    /// <param name="action">The asynchronous operation-result factory to execute.</param>
    /// <param name="timeout">The optional maximum wait time.</param>
    /// <returns>A value task that resolves to the flattened result. The outer execution elapsed time is used when flattening.</returns>
    public static async ValueTask<OperationResult> ExecuteValueAsync(Func<ValueTask<OperationResult>> action, TimeSpan? timeout = null)
    {
        Check.NotNull(action);
        return (await ExecuteValueAsync<OperationResult>(action, timeout).NoCapture()).Flatten();
    }

    /// <summary>
    /// Awaits a <see cref="ValueTask"/> operation and converts its completion, exception, cancellation, or timeout into an <see cref="OperationResult"/>.
    /// </summary>
    /// <param name="action">The asynchronous operation to execute.</param>
    /// <param name="timeout">The optional maximum wait time.</param>
    /// <returns>A value task that resolves to the operation result.</returns>
    public static async ValueTask<OperationResult> ExecuteValueAsync(Func<ValueTask> action, TimeSpan? timeout = null)
    {
        Check.NotNull(action);

        var watch = ValueStopwatch.StartNew();
        try
        {
            await TaskHelper.RunValueTaskAsync(_ => action(), timeout).NoCapture();
            return Success(watch.GetElapsedTime());
        }
        catch (Exception ex)
        {
            return (ex, watch.GetElapsedTime());
        }
    }

    /// <summary>
    /// Awaits a typed <see cref="ValueTask{TResult}"/> operation-result factory and flattens the nested result.
    /// </summary>
    /// <typeparam name="T">The value type returned by the inner result.</typeparam>
    /// <param name="action">The asynchronous operation-result factory to execute.</param>
    /// <param name="timeout">The optional maximum wait time.</param>
    /// <returns>A value task that resolves to the flattened result. The outer execution elapsed time is used when flattening.</returns>
    public static async ValueTask<OperationResult<T>> ExecuteValueAsync<T>(Func<ValueTask<OperationResult<T>>> action, TimeSpan? timeout = null)
    {
        Check.NotNull(action);
        return (await ExecuteValueAsync<OperationResult<T>>(action, timeout).NoCapture()).Flatten();
    }
}
