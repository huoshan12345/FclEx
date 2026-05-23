namespace FclEx.Actions;

public static class SeriesAction
{
    /// <summary>
    /// Creates an action that runs actions in series.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="actions">The actions to run in enumeration order.</param>
    /// <returns>An action that returns all successful values in order.</returns>
    public static IAction<T[]> Create<T>(IEnumerable<IAction<T>> actions)
    {
        return new SeriesAction<T>(actions);
    }
}

public class SeriesAction<T> : IAction<T[]>
{
    private readonly IEnumerable<IAction<T>> _actions;

    /// <summary>
    /// Creates an action that runs actions in series.
    /// </summary>
    /// <param name="actions">The actions to run in enumeration order.</param>
    public SeriesAction(IEnumerable<IAction<T>> actions)
    {
        _actions = Check.NotNull(actions);
    }

    /// <summary>
    /// Executes each action in order until one fails.
    /// </summary>
    /// <param name="token">The cancellation token passed to each action.</param>
    /// <returns>All successful values, or the first failure.</returns>
    public async Task<OperationResult<T[]>> ExecuteAsync(CancellationToken token = default)
    {
        var watch = ValueStopwatch.StartNew();

        var items = new List<T>();

        foreach (var action in _actions)
        {
            var result = await action.ExecuteAsync(token);
            if (result.IsError)
                return (result.Exception, watch.GetElapsedTime());

            items.Add(result.Value);
        }

        return (items.ToArray(), watch.GetElapsedTime());
    }
}
