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

    private bool EqualsCore(object? x, object? y)
    {
        return Comparer.TryEquals(x, y, out var result)
            ? result.Value
            : _comparer.Equals((T)x, (T)y);
    }

    public new bool Equals(object? x, object? y)
    {
        return EqualsCore(x, y);
    }

    public int GetHashCode(object obj)
    {
        return _comparer.GetHashCode((T)obj);
    }
}

