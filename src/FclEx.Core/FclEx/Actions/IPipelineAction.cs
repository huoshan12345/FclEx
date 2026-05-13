
namespace FclEx.Actions;

public interface IPipelineAction<T> : IAction<T>
{
    Task<OperationResult<T>> ExecuteActionAsync(CancellationToken token = default);

    string GetName()
#if NET6_0_OR_GREATER
        => DefaultPipelineAction.GetName(this);
#else
    ;
#endif

    Task<OperationResult<T>> HandleCancellationAsync(Exception ex)
#if NET6_0_OR_GREATER
        => DefaultPipelineAction.HandleCancellationAsync(this, ex);
#else
    ;
#endif

    Task<OperationResult<T>> HandleErrorAsync(Exception ex)
#if NET6_0_OR_GREATER
        => DefaultPipelineAction.HandleErrorAsync(this, ex);
#else
    ;
#endif

#if NET6_0_OR_GREATER
    Task<OperationResult<T>> IAction<T>.ExecuteAsync(CancellationToken token)
        => DefaultPipelineAction.ExecuteAsync(this, token);
#endif
}

public static class DefaultPipelineAction
{
    public static string GetName<T>(IPipelineAction<T> action) => action.GetType().ShortName();
    public static Task<OperationResult<T>> HandleCancellationAsync<T>(IPipelineAction<T> action, Exception ex) => Operation.Cancel<T>(ex);
    public static Task<OperationResult<T>> HandleErrorAsync<T>(IPipelineAction<T> action, Exception ex) => Operation.Error<T>(ex);
    public static Task<OperationResult<T>> ExecuteAsync<T>(IPipelineAction<T> action, CancellationToken token)
    {
        var time = ValueStopwatch.StartNew();
        Trace.WriteLine($"[{action.GetName()}]Begin");

        var future = Operation.Action(action.ExecuteActionAsync)
            .ThenResult<T, T>(r => r.IsSuccess
                ? new SuccessAction<T>(r.Value, r.Elapsed)
                : r.IsCanceled()
                    ? Operation.Action(_ => action.HandleCancellationAsync(r.Exception))
                    : Operation.Action(_ => action.HandleErrorAsync(r.Exception)));

        return future.ThenResult(m =>
        {
            var r = m.Elapsed(time.GetElapsedTime());
            Trace.WriteLine($"[{action.GetName()}]End, after {r.Elapsed.TotalMilliseconds:f3} ms]");
            return r;
        }).ExecuteAsync(token);
    }
}

public abstract class PipelineAction<T> : IPipelineAction<T>
{
    public abstract Task<OperationResult<T>> ExecuteActionAsync(CancellationToken token = default);
    public virtual string GetName() => DefaultPipelineAction.GetName(this);
    public virtual Task<OperationResult<T>> ExecuteAsync(CancellationToken token = default)
        => DefaultPipelineAction.ExecuteAsync(this, token);
    public virtual Task<OperationResult<T>> HandleCancellationAsync(Exception ex)
        => DefaultPipelineAction.HandleCancellationAsync(this, ex);
    public virtual Task<OperationResult<T>> HandleErrorAsync(Exception ex)
        => DefaultPipelineAction.HandleErrorAsync(this, ex);
}