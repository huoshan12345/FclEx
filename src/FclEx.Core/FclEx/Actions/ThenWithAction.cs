namespace FclEx.Actions;

public class ThenWithAction<T, TNext> : IAction<(T, TNext)>
{
    private readonly IAction<T> _action;
    private readonly Func<T, IAction<TNext>?> _next;
    private readonly bool _errorWhenNextNull;
    private readonly bool _prevWhenNextError;

    /// <summary>
    /// Creates an action that returns both the current and next successful values.
    /// </summary>
    /// <param name="action">The source action.</param>
    /// <param name="next">Creates the next action from the successful source value.</param>
    /// <param name="errorWhenNextNull">Whether a <see langword="null"/> next action should fail the result.</param>
    /// <param name="prevWhenNextError">Whether to keep the source value when the next action fails.</param>
    public ThenWithAction(IAction<T> action, Func<T, IAction<TNext>?> next,
        bool errorWhenNextNull = true, bool prevWhenNextError = false)
    {
        _action = Check.NotNull(action);
        _next = Check.NotNull(next);
        _errorWhenNextNull = errorWhenNextNull;
        _prevWhenNextError = prevWhenNextError;
    }

    /// <summary>
    /// Executes the source action and then the next action, returning both values.
    /// </summary>
    /// <param name="token">The cancellation token passed to both actions.</param>
    /// <returns>A tuple of both successful values, or a failure result.</returns>
    /// <remarks>The next action is not created when the source action fails.</remarks>
    public async Task<OperationResult<(T, TNext)>> ExecuteAsync(CancellationToken token = default)
    {
        var result = await _action.ExecuteAsync(token);
        if (!result.IsSuccess)
            return result.Cast<(T, TNext)>();

        var item = result.Value!;
        var nextActor = _next(item);
        if (nextActor == null)
        {
            return _errorWhenNextNull
                ? (OperationResult<(T, TNext)>)Constants.NullNextError
                : ((item, default!), result.Elapsed);
        }

        var nextResult = await nextActor.ExecuteAsync(token);
        if (!nextResult.IsSuccess)
            return _prevWhenNextError
                ? ((item, default!), result.Elapsed)
                : nextResult.Cast<(T, TNext)>();

        return ((item, nextResult.Value!), result.Elapsed + nextResult.Elapsed);
    }
}
