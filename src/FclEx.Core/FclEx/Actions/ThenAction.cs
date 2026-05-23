namespace FclEx.Actions;

public class ThenAction<T, TDest> : IAction<TDest>
{
    private readonly IAction<T> _action;
    private readonly Func<T, IAction<TDest>> _next;

    /// <summary>
    /// Creates an action that runs a next action after success.
    /// </summary>
    /// <param name="action">The source action.</param>
    /// <param name="next">Creates the next action from the successful value.</param>
    public ThenAction(IAction<T> action, Func<T, IAction<TDest>> next)
    {
        _action = Check.NotNull(action);
        _next = Check.NotNull(next);
    }

    /// <summary>
    /// Executes the source action and then the next action on success.
    /// </summary>
    /// <param name="token">The cancellation token passed to both actions.</param>
    /// <returns>The next action result, or the source failure.</returns>
    /// <remarks>If <c>next</c> returns <see langword="null"/>, an error result is returned.</remarks>
    public async Task<OperationResult<TDest>> ExecuteAsync(CancellationToken token = default)
    {
        var result = await _action.ExecuteAsync(token);
        if (result.IsSuccess == false)
            return result.Cast<TDest>();

        var nextActor = _next(result.Value);

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (nextActor is null)
            return Constants.NullNextError;

        var nextResult = await nextActor.ExecuteAsync(token);
        return nextResult.Elapsed(result.Elapsed + nextResult.Elapsed);
    }
}
