
namespace FclEx.Actions;

public interface IPipelineAction<T> : IAction<T>
{
    /// <summary>
    /// Executes the core pipeline action logic.
    /// </summary>
    /// <param name="token">The cancellation token passed to the core action.</param>
    /// <returns>The raw core action result before pipeline handlers run.</returns>
    Task<OperationResult<T>> ExecuteCoreAsync(CancellationToken token = default);

    /// <summary>
    /// Gets the display name used for tracing.
    /// </summary>
    /// <returns>The display name for trace output.</returns>
    string GetName()
#if NET6_0_OR_GREATER
        => DefaultPipelineAction.GetName(this);
#else
    ;
#endif

    /// <summary>
    /// Handles a cancellation result from the core action.
    /// </summary>
    /// <param name="exception">The cancellation exception from the core action.</param>
    /// <returns>The result returned after cancellation handling.</returns>
    Task<OperationResult<T>> HandleCancellationAsync(Exception exception)
#if NET6_0_OR_GREATER
        => DefaultPipelineAction.HandleCancellationAsync(this, exception);
#else
    ;
#endif

    /// <summary>
    /// Handles an error result from the core action.
    /// </summary>
    /// <param name="exception">The exception from the core action.</param>
    /// <returns>The result returned after error handling.</returns>
    Task<OperationResult<T>> HandleErrorAsync(Exception exception)
#if NET6_0_OR_GREATER
        => DefaultPipelineAction.HandleErrorAsync(this, exception);
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
    /// <summary>
    /// Gets the default tracing name for a pipeline action.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="action">The pipeline action.</param>
    /// <returns>The short type name of the action.</returns>
    public static string GetName<T>(IPipelineAction<T> action) 
        => action.GetType().ShortName();

    /// <summary>
    /// Converts a cancellation exception into a canceled result.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="action">The pipeline action.</param>
    /// <param name="exception">The cancellation exception.</param>
    /// <returns>A canceled operation result.</returns>
    public static Task<OperationResult<T>> HandleCancellationAsync<T>(IPipelineAction<T> action, Exception exception)
        => Operation.Cancel<T>(exception);

    /// <summary>
    /// Converts an exception into an error result.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="action">The pipeline action.</param>
    /// <param name="exception">The exception.</param>
    /// <returns>An error operation result.</returns>
    public static Task<OperationResult<T>> HandleErrorAsync<T>(IPipelineAction<T> action, Exception exception)
        => Operation.Error<T>(exception);

    /// <summary>
    /// Executes a pipeline action with tracing and error handling.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="action">The pipeline action to execute.</param>
    /// <param name="token">The cancellation token passed to the action.</param>
    /// <returns>The handled operation result.</returns>
    public static Task<OperationResult<T>> ExecuteAsync<T>(IPipelineAction<T> action, CancellationToken token)
    {
        var time = ValueStopwatch.StartNew();
        Trace.WriteLine($"[{action.GetName()}]Begin");

        var future = Operation.Action(action.ExecuteCoreAsync)
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
    /// <summary>
    /// Executes the core pipeline action logic.
    /// </summary>
    /// <param name="token">The cancellation token passed to the core action.</param>
    /// <returns>The raw core action result before pipeline handlers run.</returns>
    public abstract Task<OperationResult<T>> ExecuteCoreAsync(CancellationToken token = default);

    /// <summary>
    /// Gets the display name used for tracing.
    /// </summary>
    /// <returns>The display name for trace output.</returns>
    public virtual string GetName() => DefaultPipelineAction.GetName(this);

    /// <summary>
    /// Executes the action with pipeline handling.
    /// </summary>
    /// <param name="token">The cancellation token passed to the action.</param>
    /// <returns>The handled operation result.</returns>
    public virtual Task<OperationResult<T>> ExecuteAsync(CancellationToken token = default)
        => DefaultPipelineAction.ExecuteAsync(this, token);

    /// <summary>
    /// Handles a cancellation result from the core action.
    /// </summary>
    /// <param name="exception">The cancellation exception from the core action.</param>
    /// <returns>The result returned after cancellation handling.</returns>
    public virtual Task<OperationResult<T>> HandleCancellationAsync(Exception exception)
        => DefaultPipelineAction.HandleCancellationAsync(this, exception);

    /// <summary>
    /// Handles an error result from the core action.
    /// </summary>
    /// <param name="exception">The exception from the core action.</param>
    /// <returns>The result returned after error handling.</returns>
    public virtual Task<OperationResult<T>> HandleErrorAsync(Exception exception)
        => DefaultPipelineAction.HandleErrorAsync(this, exception);
}
