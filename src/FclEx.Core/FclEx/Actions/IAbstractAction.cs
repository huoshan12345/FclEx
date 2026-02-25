#if NET6_0_OR_GREATER
namespace FclEx.Actions;

public interface IAbstractAction<T> : IAction<T>
{
    Task<OperationResult<T>> ExecuteActionAsync(CancellationToken token = default);

    string GetName() => GetType().ShortName();
    Task<OperationResult<T>> HandleCancellationAsync(Exception ex) => Operation.Cancel<T>(ex);
    Task<OperationResult<T>> HandleErrorAsync(Exception ex) => Operation.Error<T>(ex);

    async Task<OperationResult<T>> IAction<T>.ExecuteAsync(CancellationToken token)
    {
        var time = ValueStopwatch.StartNew();
        Debug.WriteLine($"[{GetName()}]Begin");

        var future = Operation.Action(ExecuteActionAsync)
            .NextResult<T, T>(r => r.IsSuccess
                ? new SuccessAction<T>(r.Value, r.Elapsed)
                : r.IsCanceled()
                    ? Operation.Action(t => HandleCancellationAsync(r.Exception))
                    : Operation.Action(t => HandleErrorAsync(r.Exception)));

        var result = await future.ExecuteAsync(token);
        result = result.Elapsed(time.GetElapsedTime());

        Debug.WriteLine($"[{GetName()}]End, after {result.Elapsed.TotalMilliseconds:f3} ms]");
        return result;
    }
}
#endif