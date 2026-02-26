namespace FclEx.Actions;

public static class ResultAction
{
    public static ResultAction<T> Create<T>(OperationResult<T> result) => new(result);
}

public readonly struct ResultAction<T>(OperationResult<T> result) : IAction<T>
{
    public Task<OperationResult<T>> ExecuteAsync(CancellationToken token = default)
    {
        return result;
    }
}