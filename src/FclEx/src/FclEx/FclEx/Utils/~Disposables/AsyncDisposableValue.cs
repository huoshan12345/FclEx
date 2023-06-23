namespace FclEx.Utils;

public readonly record struct AsyncDisposableValue<T>(T Value, Func<T, ValueTask>? DisposeAction = null) : IAsyncDisposable
{
    public static implicit operator T(AsyncDisposableValue<T> disposable) => disposable.Value;

    public async ValueTask DisposeAsync()
    {
        if (DisposeAction != null)
        {
            await DisposeAction.Invoke(Value);
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