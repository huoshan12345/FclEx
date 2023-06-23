namespace FclEx.Utils;

public readonly record struct DisposableValue<T>(T Value, Action<T>? DisposeAction = null) : IDisposable
{
    public static implicit operator T(DisposableValue<T> disposable) => disposable.Value;

    public void Dispose()
    {
        if (DisposeAction != null)
        {
            DisposeAction.Invoke(Value);
        }
        else if (Value is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}

public static class DisposableValueExtensions
{
    public static DisposableValue<T> AsDisposable<T>(this T value, Action<T>? disposeAction = null)
    {
        return new(value, disposeAction);
    }
}