namespace FclEx.Actions;

public readonly struct NextResultAction<T, TNext> : IAction<TNext>
{
    private readonly IAction<T> _action;
    private readonly Func<OperationResult<T>, IAction<TNext>?> _next;

    public NextResultAction(IAction<T> action, Func<OperationResult<T>, IAction<TNext>?> next)
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

public readonly struct NextResultAction<T> : IAction<T>
{
    private readonly IAction<T> _action;
    private readonly Func<OperationResult<T>, IAction<T>?> _next;
    private readonly bool _errorWhenNextNull;

    public NextResultAction(IAction<T> action, Func<OperationResult<T>, IAction<T>?> next, bool errorWhenNextNull = true)
    {
        _action = Check.NotNull(action);
        _next = Check.NotNull(next);
        _errorWhenNextNull = errorWhenNextNull;
    }

    public async Task<OperationResult<T>> ExecuteAsync(CancellationToken token = default)
    {
        var result = await _action.ExecuteAsync(token);

        var nextActor = _next(result);
        if (nextActor == null)
        {
            return _errorWhenNextNull
                ? Constants.NullNextError
                : result;
        }

        var nextResult = await nextActor.ExecuteAsync(token);
        return nextResult.Elapsed(result.Elapsed + nextResult.Elapsed);
    }
}