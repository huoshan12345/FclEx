namespace FclEx.Actions;

public readonly struct SuccessAction<T> : IAction<T>
{
    private readonly T _obj;
    private readonly TimeSpan _timeSpan;

    public SuccessAction(T obj, TimeSpan timeSpan = default)
    {
        _obj = obj;
        _timeSpan = timeSpan;
    }

    public Task<OperationResult<T>> ExecuteAsync(CancellationToken token = default)
    {
        return Operation.Success(_obj, _timeSpan);
    }
}