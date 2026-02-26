namespace FclEx.Actions;

public class MapValueAction<T, TDest> : IAction<TDest>
{
    private readonly IAction<T> _action;
    private readonly Func<T, TDest> _map;

    public MapValueAction(IAction<T> action, Func<T, TDest> map)
    {
        _action = Check.NotNull(action);
        _map = Check.NotNull(map);
    }

    public async Task<OperationResult<TDest>> ExecuteAsync(CancellationToken token = default)
    {
        var result = await _action.ExecuteAsync(token);
        return result.MapValue(_map);
    }
}