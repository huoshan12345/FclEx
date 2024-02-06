namespace FclEx.Utils;

public readonly struct CompositeDisposable<T> : IDisposable where T : IDisposable
{
    private readonly ICollection<T> _disposables;

    public CompositeDisposable(IEnumerable<T> enumerable)
    {
        _disposables = enumerable.EmptyIfNull().AsICollection(); // cannot use IEnumerable<T> here.
    }

    public void Dispose()
    {
        foreach (var e in _disposables.EmptyIfNull())
            e?.Dispose();
    }
}

public static class Extensions
{
    public static CompositeDisposable<T> Composite<T>(this IEnumerable<T> enumerable) where T : IDisposable
    {
        return new CompositeDisposable<T>(enumerable);
    }
}