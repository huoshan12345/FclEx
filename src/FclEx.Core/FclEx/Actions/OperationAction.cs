namespace FclEx.Actions;

public class OperationAction<T> : IAction<T>
{
    private readonly Func<CancellationToken, Task<OperationResult<T>>> _execute;

    /// <summary>
    /// Creates an action from an async operation delegate.
    /// </summary>
    /// <param name="execute">The delegate invoked when the action executes.</param>
    public OperationAction(Func<CancellationToken, Task<OperationResult<T>>> execute)
    {
        _execute = Check.NotNull(execute);
    }

    /// <summary>
    /// Executes the wrapped operation delegate.
    /// </summary>
    /// <param name="token">The cancellation token passed to the delegate.</param>
    /// <returns>The result returned by the delegate.</returns>
    public Task<OperationResult<T>> ExecuteAsync(CancellationToken token = default)
    {
        return _execute(token);
    }
}
