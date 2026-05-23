namespace FclEx.Utils;

public class AsyncDisposable : IAsyncDisposable
{
    private readonly Func<Task> _disposeBody;

    public AsyncDisposable(Func<Task> disposeBody)
    {
        _disposeBody = disposeBody ?? throw new ArgumentNullException(nameof(disposeBody));
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return new(_disposeBody());
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