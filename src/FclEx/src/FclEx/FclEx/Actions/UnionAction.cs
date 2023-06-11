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

    public async Task<OperateResult<(T, TNext)>> ExecuteAsync(CancellationToken token = default)
    {
        var result = await _action.ExecuteAsync(token).DonotCapture();
        if (!result.Success)
            return result.ToExplicit<(T, TNext)>();

        var item = result.Value!;
        var nextActor = _next(item);
        if (nextActor == null)
        {
            return _errorWhenNextNull
                ? (OperateResult<(T, TNext)>)Constant.NullNextError
                : ((item, default!), result.Elapsed);
        }

        var nextResult = await nextActor.ExecuteAsync(token).DonotCapture();
        if (!nextResult.Success)
            return _prevWhenNextError
                ? ((item, default!), result.Elapsed)
                : nextResult.ToExplicit<(T, TNext)>();

        return ((item, nextResult.Value!), result.Elapsed + nextResult.Elapsed);
    }
}