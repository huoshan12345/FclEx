namespace FclEx.Utils;

public class DisposableValue<T>(T value, Action<T>? disposeAction = null) : IDisposable
{
    private int _disposeStarted;

    public T Value => Volatile.Read(ref _disposeStarted) != 0
        ? throw new ObjectDisposedException(nameof(Value))
        : value;

    public static implicit operator T(DisposableValue<T> disposable) => disposable.Value;

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposeStarted, 1) != 0)
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
    }
}
