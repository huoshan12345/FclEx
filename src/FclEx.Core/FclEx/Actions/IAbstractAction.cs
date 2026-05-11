
namespace FclEx.Actions;

public interface IAbstractAction<T> : IAction<T>
{
    Task<OperationResult<T>> ExecuteActionAsync(CancellationToken token = default);

    string GetName()
#if NET6_0_OR_GREATER
        => GetType().ShortName();
#else
    ;
#endif

    Task<OperationResult<T>> HandleCancellationAsync(Exception ex)
#if NET6_0_OR_GREATER
        => Operation.Cancel<T>(ex);
#else
    ;
#endif

    Task<OperationResult<T>> HandleErrorAsync(Exception ex)
#if NET6_0_OR_GREATER
        => Operation.Error<T>(ex);
#else
    ;
#endif

#if NET6_0_OR_GREATER
    async Task<OperationResult<T>> IAction<T>.ExecuteAsync(CancellationToken token)
    {
        var time = ValueStopwatch.StartNew();
        Debug.WriteLine($"[{GetName()}]Begin");

        var future = Operation.Action(ExecuteActionAsync)
            .ThenResult<T, T>(r => r.IsSuccess
                ? new SuccessAction<T>(r.Value, r.Elapsed)
                : r.IsCanceled()
                    ? Operation.Action(t => HandleCancellationAsync(r.Exception))
                    : Operation.Action(t => HandleErrorAsync(r.Exception)));

        var result = await future.ExecuteAsync(token);
        result = result.Elapsed(time.GetElapsedTime());

        Debug.WriteLine($"[{GetName()}]End, after {result.Elapsed.TotalMilliseconds:f3} ms]");
        return result;
    }
#endif
}