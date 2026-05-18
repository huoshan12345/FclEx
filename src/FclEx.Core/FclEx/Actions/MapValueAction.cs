namespace FclEx.Actions;

public class MapValueAction<T, TDest> : IAction<TDest>
{
    private readonly IAction<T> _action;
    private readonly Func<T, TDest> _map;

    /// <summary>
    /// Creates an action that maps a successful value.
    /// </summary>
    /// <param name="action">The source action.</param>
    /// <param name="map">The mapper invoked only when the source action succeeds.</param>
    public MapValueAction(IAction<T> action, Func<T, TDest> map)
    {
        _action = Check.NotNull(action);
        _map = Check.NotNull(map);
    }

    /// <summary>
    /// Executes the source action and maps its successful value.
    /// </summary>
    /// <param name="token">The cancellation token passed to the source action.</param>
    /// <returns>The mapped successful result, or the source failure.</returns>
    public async Task<OperationResult<TDest>> ExecuteAsync(CancellationToken token = default)
    {
        var result = await _action.ExecuteAsync(token);
        return result.MapValue(_map);
    }
}
