namespace FclEx.Utils;

public class AsyncDisposableValue<T> : IAsyncDisposable
{
    private readonly T _value;
    private readonly Func<T, ValueTask>? _disposeAction;
    private Task? _disposeTask;

    public AsyncDisposableValue(T value, Func<T, ValueTask>? disposeAction = null)
    {
        _value = value;
        _disposeAction = disposeAction;
    }

    public T Value => Volatile.Read(ref _disposeTask) is not null
        ? throw new ObjectDisposedException(nameof(Value))
        : _value;

    public static implicit operator T(AsyncDisposableValue<T> disposable) => disposable.Value;

    public ValueTask DisposeAsync()
    {
        var disposeTask = Volatile.Read(ref _disposeTask);
        if (disposeTask is not null)
            return new ValueTask(disposeTask);

        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        disposeTask = Interlocked.CompareExchange(ref _disposeTask, completion.Task, null);
        if (disposeTask is not null)
            return new ValueTask(disposeTask);

        GC.SuppressFinalize(this);
        _ = CompleteDisposalAsync(completion);
        return new ValueTask(completion.Task);
    }

    private async Task CompleteDisposalAsync(TaskCompletionSource completion)
    {
        try
        {
            if (_disposeAction is not null)
            {
                await _disposeAction.Invoke(_value).NoCapture();
            }
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            else if (_value is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync().NoCapture();
            }
            else if (_value is IDisposable disposable)
            {
                disposable.Dispose();
            }

            completion.TrySetResult();
        }
        catch (OperationCanceledException)
        {
            completion.TrySetCanceled();
        }
        catch (Exception ex)
        {
            completion.TrySetException(ex);
        }
    }
}
