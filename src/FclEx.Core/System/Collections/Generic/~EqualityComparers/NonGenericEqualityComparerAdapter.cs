namespace System.Collections.Generic;

public class NonGenericEqualityComparerAdapter
{
    public static NonGenericEqualityComparerAdapter<T> Create<T>(IEqualityComparer<T> comparer) => new(comparer);
}

public class NonGenericEqualityComparerAdapter<T> : IEqualityComparer, IEqualityComparer<object>
{
    private readonly IEqualityComparer<T> _comparer;

    public NonGenericEqualityComparerAdapter(IEqualityComparer<T>? comparer = null)
    {
        _comparer = comparer ?? EqualityComparer<T>.Default;
    }

    public new bool Equals(object? x, object? y)
    {
        return ComparerHelper.TryEquals(x, y, out var result)
            ? result.Value
            : _comparer.Equals((T)x, (T)y);
    }

    public int GetHashCode(object obj)
    {
        return _comparer.GetHashCode((T)obj);
    }
}

