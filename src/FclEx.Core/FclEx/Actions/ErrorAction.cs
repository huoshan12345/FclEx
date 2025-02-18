namespace FclEx.Actions;

public static class ErrorAction
{
    public static ErrorAction<T> Create<T>(string error, TimeSpan timeSpan = default) => new(error, timeSpan);
    public static ErrorAction<T> Create<T>(Exception ex, TimeSpan timeSpan = default) => new(ex, timeSpan);
}

public readonly struct ErrorAction<T> : IAction<T>
{
    private readonly OperationResult<T> _result;

    public ErrorAction(string error, TimeSpan timeSpan = default)
    {
        _result = Operation.Error<T>(error, timeSpan);
    }

    public ErrorAction(Exception ex, TimeSpan timeSpan = default)
    {
        _result = Operation.Error<T>(ex, timeSpan);
    }

    public Task<OperationResult<T>> ExecuteAsync(CancellationToken token = default)
    {
        return _result;
    }
}