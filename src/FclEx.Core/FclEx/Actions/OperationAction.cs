namespace FclEx.Actions;

public class OperationAction<T> : IAction<T>
{
    private readonly Func<CancellationToken, Task<OperationResult<T>>> _func;

    public OperationAction(Func<CancellationToken, Task<OperationResult<T>>> func)
    {
        _func = Check.NotNull(func);
    }

    public Task<OperationResult<T>> ExecuteAsync(CancellationToken token = default)
    {
        return _func(token);
    }
}