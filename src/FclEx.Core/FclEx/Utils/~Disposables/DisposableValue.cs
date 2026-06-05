namespace FclEx.Utils;

public class DisposableValue<T>(T value, Action<T>? disposeAction = null) : IDisposable
{
    private volatile bool _disposed;

    public T Value => _disposed
        ? throw new ObjectDisposedException(nameof(Value))
        : value;

    public static implicit operator T(DisposableValue<T> disposable) => disposable.Value;

    public void Dispose()
    {
        if (_disposed)
            return;

        GC.SuppressFinalize(this);

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