#if !NET5_0_OR_GREATER
namespace System.Threading.Tasks;

public class TaskCompletionSource
{
    private readonly TaskCompletionSource<bool> _inner;

    public TaskCompletionSource()
        => _inner = new TaskCompletionSource<bool>();

    public TaskCompletionSource(TaskCreationOptions creationOptions)
        => _inner = new TaskCompletionSource<bool>(creationOptions);

    public TaskCompletionSource(object? state)
        => _inner = new TaskCompletionSource<bool>(state);

    public TaskCompletionSource(object? state, TaskCreationOptions creationOptions)
        => _inner = new TaskCompletionSource<bool>(state, creationOptions);

    /// <summary>与此 TaskCompletionSource 关联的 Task。</summary>
    public Task Task => _inner.Task;

    [MethodImpl(AggressiveInlining)]
    public void SetResult() => _inner.SetResult(true);

    [MethodImpl(AggressiveInlining)]
    public bool TrySetResult() => _inner.TrySetResult(true);

    [MethodImpl(AggressiveInlining)]
    public void SetException(Exception exception) => _inner.SetException(exception);

    [MethodImpl(AggressiveInlining)]
    public void SetException(IEnumerable<Exception> exceptions) => _inner.SetException(exceptions);

    [MethodImpl(AggressiveInlining)]
    public bool TrySetException(Exception exception) => _inner.TrySetException(exception);

    [MethodImpl(AggressiveInlining)]
    public bool TrySetException(IEnumerable<Exception> exceptions) => _inner.TrySetException(exceptions);

    [MethodImpl(AggressiveInlining)]
    public bool TrySetCanceled() => _inner.TrySetCanceled();

    [MethodImpl(AggressiveInlining)]
    public bool TrySetCanceled(CancellationToken cancellationToken) => _inner.TrySetCanceled(cancellationToken);

    [MethodImpl(AggressiveInlining)]
    public void SetCanceled() => _inner.SetCanceled();

    [MethodImpl(AggressiveInlining)]
    public void SetCanceled(CancellationToken cancellationToken)
    {
        if (_inner.TrySetCanceled(cancellationToken) == false)
            throw new InvalidOperationException("An attempt was made to transition a task to a final state when it had already completed.");
    }
}

#endif