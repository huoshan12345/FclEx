namespace FclEx.Utils;

public class Disposable : IDisposable
{
    private Action? _disposeBody;

    public Disposable(Action disposeBody)
    {
        _disposeBody = disposeBody ?? throw new ArgumentNullException(nameof(disposeBody));
    }

    public void Dispose()
    {
        var disposeBody = Interlocked.Exchange(ref _disposeBody, null);
        if (disposeBody is null)
            return;

        GC.SuppressFinalize(this);
        disposeBody();
    }

    public static IDisposable Empty => Create(() => { });
    public static IDisposable Create(Action action) => new Disposable(action);

    public static DisposableValue<T> FromValue<T>(T value, Action<T>? disposeAction = null)
    {
        return new(value, disposeAction);
    }
}
