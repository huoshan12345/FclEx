namespace FclEx.Comparers;

public class ComparerBuilder
{
    public static ComparerBuilder<T> For<T>()
    {
        return new();
    }
}

public class ComparerBuilder<T> : IComparerBuilder<T>
{
    private IComparer<T>? _comparer = null;

    public IComparer<T> Build()
    {
        return _comparer ?? Comparer<T>.Default;
    }

    public ComparerBuilder<T> Set(IComparer<T> comparer)
    {
        _comparer = comparer;
        return this;
    }
}