namespace FclEx.Actions;

public readonly struct UnionAction<T, TNext> : IAction<(T, TNext)>
{
    private readonly IAction<T> _action;
    private readonly Func<T, IAction<TNext>?> _next;
    private readonly bool _errorWhenNextNull;
    private readonly bool _prevWhenNextError;

    public UnionAction(IAction<T> action, Func<T, IAction<TNext>?> next,
        bool errorWhenNextNull = true, bool prevWhenNextError = false)
    {
        _action = Check.NotNull(action);
        _next = Check.NotNull(next);
        _errorWhenNextNull = errorWhenNextNull;
        _prevWhenNextError = prevWhenNextError;
    }

    public async Task<OperationResult<(T, TNext)>> ExecuteAsync(CancellationToken token = default)
    {
        var result = await _action.ExecuteAsync(token).IgnoreSyncContext();
        if (!result.Success)
            return result.CastTo<(T, TNext)>();

        var item = result.Value!;
        var nextActor = _next(item);
        if (nextActor == null)
        {
            return _errorWhenNextNull
                ? (OperationResult<(T, TNext)>)Constants.NullNextError
                : ((item, default!), result.Elapsed);
        }

        var nextResult = await nextActor.ExecuteAsync(token).IgnoreSyncContext();
        if (!nextResult.Success)
            return _prevWhenNextError
                ? ((item, default!), result.Elapsed)
                : nextResult.CastTo<(T, TNext)>();

        return ((item, nextResult.Value!), result.Elapsed + nextResult.Elapsed);
    }
}