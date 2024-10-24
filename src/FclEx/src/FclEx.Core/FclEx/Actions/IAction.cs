namespace FclEx.Actions;

public interface IAction<T>
{
    Task<OperateResult<T>> ExecuteAsync(CancellationToken token = default);
}