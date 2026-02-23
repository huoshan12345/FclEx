namespace FclEx.Actions;

public class ThenResultAction<T, TNext> : IAction<TNext>
{
    private readonly IAction<T> _action;
    private readonly Func<OperationResult<T>, IAction<TNext>?> _next;

    public ThenResultAction(IAction<T> action, Func<OperationResult<T>, IAction<TNext>?> next)
    {
        _action = Check.NotNull(action);
        _next = Check.NotNull(next);
    }

    public async Task<OperationResult<TNext>> ExecuteAsync(CancellationToken token = default)
    {
        var result = await _action.ExecuteAsync(token);

        var nextActor = _next(result);
        if (nextActor == null)
            return Constants.NullNextError;

        var nextResult = await nextActor.ExecuteAsync(token);
        return nextResult.Elapsed(result.Elapsed + nextResult.Elapsed);
    }
}