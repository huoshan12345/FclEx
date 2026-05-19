namespace FclEx.Actions;

public static class ParallelAction
{
    /// <summary>
    /// Creates an action that runs actions in parallel.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="actions">The actions to run concurrently.</param>
    /// <returns>An action that returns all successful values in input order.</returns>
    public static IAction<T[]> Create<T>(IEnumerable<IAction<T>> actions)
    {
        return new ParallelAction<T>(actions);
    }
}

public class ParallelAction<T> : IAction<T[]>
{
    private readonly IEnumerable<IAction<T>> _actions;

    /// <summary>
    /// Creates an action that runs actions in parallel.
    /// </summary>
    /// <param name="actions">The actions to run concurrently.</param>
    public ParallelAction(IEnumerable<IAction<T>> actions)
    {
        _actions = Check.NotNull(actions);
    }

    /// <summary>
    /// Executes all actions in parallel and returns their values in order.
    /// </summary>
    /// <param name="token">The cancellation token passed to each action.</param>
    /// <returns>All successful values in input order, or the first failed result by input order.</returns>
    /// <remarks>All actions are started before failure results are inspected.</remarks>
    public async Task<OperationResult<T[]>> ExecuteAsync(CancellationToken token = default)
    {
        var watch = ValueStopwatch.StartNew();
        var results = await _actions.Select(m => m.ExecuteAsync(token)).WhenAll();
        var elapsed = watch.GetElapsedTime();

        var items = new T[results.Length];
        for (var i = 0; i < results.Length; i++)
        {
            var result = results[i];
            if (result.IsError)
                return (result.Exception, elapsed);

            items[i] = result.Value;
        }

        return (items, elapsed);
    }
}
