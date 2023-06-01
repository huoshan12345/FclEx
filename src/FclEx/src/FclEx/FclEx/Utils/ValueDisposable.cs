namespace FclEx.Utils;

public readonly struct ValueDisposable<T> : IDisposable
{
    private readonly Action<T> _disposeAction;

    public ValueDisposable(T value, Action<T>? disposeAction = null)
    {
        Value = value;
        _disposeAction = disposeAction ?? (t => (t as IDisposable)?.Dispose());
    }

    public T Value { get; }

    public void Dispose()
    {
        _disposeAction(Value);
    }

    public static implicit operator T(ValueDisposable<T> disposable)
    {
        return disposable.Value;
    }
}

public static class ValueDisposableExtensions
{
    public static ValueDisposable<T> AsDisposable<T>(this T value, Action<T>? disposeAction = null)
    {
        return new(value, disposeAction);
    }
}