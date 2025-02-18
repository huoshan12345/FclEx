namespace FclEx.Actions;

public readonly struct BindAction<T, TDest> : IAction<TDest>
{
    private readonly IAction<T> _action;
    private readonly Func<T, OperationResult<TDest>> _map;

    public BindAction(IAction<T> action, Func<T, OperationResult<TDest>> map)
    {
        _action = action ?? throw new ArgumentNullException(nameof(action));
        _map = map ?? throw new ArgumentNullException(nameof(map));
    }

    public async Task<OperationResult<TDest>> ExecuteAsync(CancellationToken token = default)
    {
        var result = await _action.ExecuteAsync(token);
        return result.Bind(_map);
    }
}