namespace FclEx.Actions;

public interface IAction<T>
{
    /// <summary>
    /// Executes the action.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token passed to the action.</param>
    /// <returns>The operation result produced by the action.</returns>
    Task<OperationResult<T>> ExecuteAsync(CancellationToken cancellationToken = default);
}
