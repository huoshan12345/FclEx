namespace FclEx.Actions;

public static class SeriesAction
{
    public static IAction<T[]> Create<T>(IEnumerable<IAction<T>> actions)
    {
        return new SeriesAction<T>(actions);
    }
}

public class SeriesAction<T> : IAction<T[]>
{
    private readonly IEnumerable<IAction<T>> _actions;

    public SeriesAction(IEnumerable<IAction<T>> actions)
    {
        _actions = Check.NotNull(actions);
    }

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
