namespace FclEx.Actions;

public class ThenResultAction<T, TNext> : IAction<TNext>
{
    private readonly IAction<T> _action;
    private readonly Func<OperationResult<T>, IAction<TNext>?> _next;

    /// <summary>
    /// Creates an action that runs a next action with the full result.
    /// </summary>
    /// <param name="action">The source action.</param>
    /// <param name="next">Creates the next action from the full source result.</param>
    public ThenResultAction(IAction<T> action, Func<OperationResult<T>, IAction<TNext>?> next)
    {
        _action = Check.NotNull(action);
        _next = Check.NotNull(next);
    }

    /// <summary>
    /// Executes the source action and then the next action with its result.
    /// </summary>
    /// <param name="token">The cancellation token passed to both actions.</param>
    /// <returns>The next action result.</returns>
    /// <remarks>The next action is created even when the source action fails.</remarks>
    public async Task<OperationResult<TNext>> ExecuteAsync(CancellationToken token = default)
    {
        var result = await _action.ExecuteAsync(token);

        var nextActor = _next(result);
        if (nextActor == null)
            return (Constants.NullNextError, result.Elapsed);

        var nextResult = await nextActor.ExecuteAsync(token);
        return nextResult.Elapsed(result.Elapsed + nextResult.Elapsed);
    }
}
