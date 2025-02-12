namespace FclEx.Actions;

public static class ResultAction
{
    public static ResultAction<T> Create<T>(OperationResult<T> result) => new(result);
    public static ResultAction<T> Create<T>(T value, TimeSpan elapsed = default) 
        => new(Operation.Success(value, elapsed));
}

public readonly struct ResultAction<T> : IAction<T>
{
    private readonly OperationResult<T> _result;

    public ResultAction(OperationResult<T> result)
    {
        _result = result;
    }

    public Task<OperationResult<T>> ExecuteAsync(CancellationToken token = default)
    {
        return _result;
    }
}