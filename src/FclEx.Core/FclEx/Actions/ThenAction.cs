namespace FclEx.Actions;

public class ThenAction<T, TDest> : IAction<TDest>
{
    private readonly IAction<T> _action;
    private readonly Func<T, IAction<TDest>> _next;

    public ThenAction(IAction<T> action, Func<T, IAction<TDest>> next)
    {
        _action = Check.NotNull(action);
        _next = Check.NotNull(next);
    }

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