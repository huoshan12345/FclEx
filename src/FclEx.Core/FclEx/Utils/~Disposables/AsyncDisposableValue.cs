namespace FclEx.Utils;

public class AsyncDisposableValue<T>(T value, Func<T, ValueTask>? disposeAction = null) : IAsyncDisposable
{
    private volatile bool _disposed;

    public T Value
    {
        get
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(Value));
            return value;
        }
    }

    public static implicit operator T(AsyncDisposableValue<T> disposable) => disposable.Value;

    public readonly Func<T, ValueTask>? _disposeAction = disposeAction;

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        if (_disposeAction != null)
        {
            await _disposeAction.Invoke(Value);
        }
        else if (Value is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
        else if (Value is IDisposable disposable)
        {
            disposable.Dispose();
        }

        _disposed = true;
    }
}