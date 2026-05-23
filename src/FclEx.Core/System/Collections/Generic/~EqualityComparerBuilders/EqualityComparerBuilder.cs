namespace System.Collections.Generic;

public class EqualityComparerBuilder
{
    public static EqualityComparerBuilder<T> For<T>()
    {
        return new();
    }
}

public class EqualityComparerBuilder<T> : IEqualityComparerBuilder<T>
{
    private IEqualityComparer<T>? _comparer = null;

    public IEqualityComparer<T> Build()
    {
        return _comparer ?? EqualityComparer<T>.Default;
    }

    public EqualityComparerBuilder<T> Set(IEqualityComparer<T> comparer)
    {
        _comparer = comparer;
        return this;
    }
}