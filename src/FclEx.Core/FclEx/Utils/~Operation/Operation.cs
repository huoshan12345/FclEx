namespace FclEx.Utils;

public static partial class Operation
{
    /// <summary>
    /// Executes a synchronous operation and converts its completion or exception into an <see cref="OperationResult"/>.
    /// </summary>
    /// <param name="action">The operation to execute.</param>
    /// <returns>A success result when <paramref name="action"/> completes, or an error result when it throws. The elapsed time is measured around the execution.</returns>
    public static OperationResult Execute(Action action)
    {
        Check.NotNull(action);

        var watch = ValueStopwatch.StartNew();
        try
        {
            action();
            return Success(watch.GetElapsedTime());
        }
        catch (Exception ex)
        {
            return (ex, watch.GetElapsedTime());
        }
    }

    /// <summary>
    /// Executes a synchronous value-producing operation and converts its value or exception into an <see cref="OperationResult{T}"/>.
    /// </summary>
    /// <typeparam name="T">The value type returned by the operation.</typeparam>
    /// <param name="action">The operation to execute.</param>
    /// <returns>A success result with the returned value, or an error result when <paramref name="action"/> throws. The elapsed time is measured around the execution.</returns>
    public static OperationResult<T> Execute<T>(Func<T> action)
    {
        Check.NotNull(action);

        var watch = ValueStopwatch.StartNew();
        try
        {
            var result = action();
            return (result, watch.GetElapsedTime());
        }
        catch (Exception ex)
        {
            return (ex, watch.GetElapsedTime());
        }
    }

    /// <summary>
    /// Executes a synchronous operation-result factory and flattens the nested result.
    /// </summary>
    /// <param name="action">The operation-result factory to execute.</param>
    /// <returns>The flattened result. When the outer execution records elapsed time, that outer elapsed time is used.</returns>
    public static OperationResult Execute(Func<OperationResult> action) => Execute<OperationResult>(action).Flatten();

    /// <summary>
    /// Executes a synchronous typed operation-result factory and flattens the nested result.
    /// </summary>
    /// <typeparam name="T">The value type returned by the inner result.</typeparam>
    /// <param name="action">The operation-result factory to execute.</param>
    /// <returns>The flattened result. When the outer execution records elapsed time, that outer elapsed time is used.</returns>
    public static OperationResult<T> Execute<T>(Func<OperationResult<T>> action) => Execute<OperationResult<T>>(action).Flatten();
}

