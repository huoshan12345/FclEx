namespace FclEx.Actions;

public static class SuccessAction
{
    public static SuccessAction<T> Create<T>(T obj, TimeSpan timeSpan = default)
    {
        return new(obj, timeSpan);
    }
}

public readonly struct SuccessAction<T>(T obj, TimeSpan timeSpan = default) : IAction<T>
{
    public Task<OperationResult<T>> ExecuteAsync(CancellationToken token = default)
    {
        return Operation.Success(obj, timeSpan);
    }
}