namespace FclEx.Actions;

public static class SuccessAction
{
    public static IAction<T> Create<T>(T obj, TimeSpan timeSpan = default)
    {
        return new SuccessAction<T>(obj, timeSpan);
    }
}

public class SuccessAction<T>(T obj, TimeSpan timeSpan = default) : IAction<T>
{
    public Task<OperationResult<T>> ExecuteAsync(CancellationToken token = default)
    {
        return Operation.Success(obj, timeSpan);
    }
}