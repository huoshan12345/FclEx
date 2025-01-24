namespace FclEx.Utils;

public class DisposableValue<T>(T value, Action<T>? disposeAction = null) : IDisposable
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

    public static implicit operator T(DisposableValue<T> disposable) => disposable.Value;

    public void Dispose()
    {
        if (_disposed)
            return;

        if (disposeAction != null)
        {
            disposeAction.Invoke(value);
        }
        else if (value is IDisposable disposable)
        {
            disposable.Dispose();
        }

        _disposed = true;
    }
}

public static class DisposableValueExtensions
{
    public static DisposableValue<T> ToDisposable<T>(this T value, Action<T>? disposeAction = null)
    {
        return new(value, disposeAction);
    }
}