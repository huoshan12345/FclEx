namespace FclEx.Actions;

public interface IAction<T>
{
    Task<OperationResult<T>> ExecuteAsync(CancellationToken token = default);
}