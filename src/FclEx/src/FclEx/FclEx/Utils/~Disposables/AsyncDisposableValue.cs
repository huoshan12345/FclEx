namespace FclEx.Utils;

public readonly struct AsyncDisposableValue<T>(T value, Func<T, ValueTask>? disposeAction = null) : IAsyncDisposable
{
    public static implicit operator T(AsyncDisposableValue<T> disposable) => disposable.Value;

    public T Value { get; } = value;
    public readonly Func<T, ValueTask>? _disposeAction = disposeAction;

    public async ValueTask DisposeAsync()
    {
        if (_disposeAction != null)
        {
            await _disposeAction.Invoke(Value);
        }
        // ReSharper disable once ConvertIfStatementToSwitchStatement
        else if (Value is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
        else if (Value is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}

public static class AsyncDisposableValueExtensions
{
    public static AsyncDisposableValue<T> AsAsyncDisposable<T>(this T value, Func<T, ValueTask>? disposeAction = null)
    {
        return new AsyncDisposableValue<T>(value, disposeAction);
    }
}