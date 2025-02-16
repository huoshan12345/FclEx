namespace FclEx.Actions;

public static class ErrorAction
{
    public static ErrorAction<T> Create<T>(string error, TimeSpan timeSpan = default) => new(error, timeSpan);
    public static ErrorAction<T> Create<T>(Exception ex, TimeSpan timeSpan = default) => new(ex, timeSpan);
}

public readonly struct ErrorAction<T> : IAction<T>
{
    private readonly string? _error;
    private readonly Exception? _ex;
    private readonly TimeSpan _timeSpan;

    public ErrorAction(string error, TimeSpan timeSpan = default)
    {
        _error = error;
        _timeSpan = timeSpan;
        _ex = null;
    }

    public ErrorAction(Exception ex, TimeSpan timeSpan = default)
    {
        _ex = ex ?? throw new ArgumentNullException(nameof(ex));
        _timeSpan = timeSpan;
        _error = null;
    }

    public Task<OperationResult<T>> ExecuteAsync(CancellationToken token = default)
    {
        return _ex is null
            ? Operation.Error<T>(_error, _timeSpan)
            : Operation.Error<T>(_ex, _timeSpan);
    }
}