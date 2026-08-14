namespace FclEx.Utils;

public class AsyncDisposable : IAsyncDisposable
{
    private Func<Task>? _disposeBody;
    private Task? _disposeTask;

    public AsyncDisposable(Func<Task> disposeBody)
    {
        _disposeBody = disposeBody ?? throw new ArgumentNullException(nameof(disposeBody));
    }

    public ValueTask DisposeAsync()
    {
        var disposeTask = Volatile.Read(ref _disposeTask);
        if (disposeTask is not null)
            return new ValueTask(disposeTask);

        var completion = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        disposeTask = Interlocked.CompareExchange(ref _disposeTask, completion.Task, null);
        if (disposeTask is not null)
            return new ValueTask(disposeTask);

        GC.SuppressFinalize(this);
        var disposeBody = Interlocked.Exchange(ref _disposeBody, null)!;
        _ = CompleteDisposalAsync(disposeBody, completion);
        return new ValueTask(completion.Task);
    }

    private static async Task CompleteDisposalAsync(
        Func<Task> disposeBody,
        TaskCompletionSource<object?> completion)
    {
        try
        {
            await disposeBody().ConfigureAwait(false);
            completion.TrySetResult(null);
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

    public static readonly IAsyncDisposable Empty = Create(() => Task.CompletedTask);
    public static readonly Task<IAsyncDisposable> EmptyTask = Task.FromResult(Empty);
    public static readonly ValueTask<IAsyncDisposable> EmptyValueTask = EmptyTask.ToValueTask();

    public static IAsyncDisposable Create(Func<Task> task) => new AsyncDisposable(task);
    public static IAsyncDisposable Create(Action action) => Create(() =>
    {
        action();
        return Task.CompletedTask;
    });

    public static AsyncDisposableValue<T> FromValue<T>(T value, Func<T, ValueTask>? disposeAction = null)
    {
        return new(value, disposeAction);
    }
}
