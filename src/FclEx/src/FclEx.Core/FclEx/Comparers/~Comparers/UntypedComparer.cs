namespace FclEx.Comparers;

public class UntypedComparer
{
    public static UntypedComparer<T> Create<T>(IComparer<T> comparer) => new(comparer);
}

public class UntypedComparer<T> : IComparer
{
    private readonly IComparer<T> _comparer;

    public UntypedComparer(IComparer<T> comparer)
    {
        _comparer = comparer;
    }

    public int Compare(object? x, object? y)
    {
        return _comparer.Compare(x.CastTo<T>()!, y.CastTo<T>()!);
    }
}