namespace FclEx.Actions;

public readonly struct NextResultAction<T, TNext> : IAction<TNext>
{
    private readonly IAction<T> _action;
    private readonly Func<OperateResult<T>, IAction<TNext>?> _next;

    public NextResultAction(IAction<T> action, Func<OperateResult<T>, IAction<TNext>?> next)
    {
        _action = Check.NotNull(action);
        _next = Check.NotNull(next);
    }

    public async Task<OperateResult<TNext>> ExecuteAsync(CancellationToken token = default)
    {
        var result = await _action.ExecuteAsync(token).IgnoreSyncContext();

        var nextActor = _next(result);
        if (nextActor == null)
            return Constant.NullNextError;

        var nextResult = await nextActor.ExecuteAsync(token).IgnoreSyncContext();
        return nextResult.Elapsed(result.Elapsed + nextResult.Elapsed);
    }
}

public readonly struct NextResultAction<T> : IAction<T>
{
    private readonly IAction<T> _action;
    private readonly Func<OperateResult<T>, IAction<T>?> _next;
    private readonly bool _errorWhenNextNull;

    public NextResultAction(IAction<T> action, Func<OperateResult<T>, IAction<T>?> next, bool errorWhenNextNull = true)
    {
        _action = Check.NotNull(action);
        _next = Check.NotNull(next);
        _errorWhenNextNull = errorWhenNextNull;
    }

    public async Task<OperateResult<T>> ExecuteAsync(CancellationToken token = default)
    {
        var result = await _action.ExecuteAsync(token).IgnoreSyncContext();

        var nextActor = _next(result);
        if (nextActor == null)
        {
            return _errorWhenNextNull
                ? Constant.NullNextError
                : result;
        }

        var nextResult = await nextActor.ExecuteAsync(token).IgnoreSyncContext();
        return nextResult.Elapsed(result.Elapsed + nextResult.Elapsed);
    }
}