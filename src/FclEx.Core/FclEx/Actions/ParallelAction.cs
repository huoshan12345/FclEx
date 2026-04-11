namespace FclEx.Actions;

public static class ParallelAction
{
    public static IAction<T[]> Create<T>(IEnumerable<IAction<T>> actions)
    {
        return new ParallelAction<T>(actions);
    }
}

public class ParallelAction<T> : IAction<T[]>
{
    private readonly IEnumerable<IAction<T>> _actions;

    public ParallelAction(IEnumerable<IAction<T>> actions)
    {
        _actions = Check.NotNull(actions);
    }

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
