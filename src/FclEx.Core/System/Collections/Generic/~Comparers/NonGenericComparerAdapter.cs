namespace System.Collections.Generic;

public class NonGenericComparerAdapter
{
    public static NonGenericComparerAdapter<T> Create<T>(IComparer<T> comparer) => new(comparer);
}

public class NonGenericComparerAdapter<T> : IComparer
{
    private readonly IComparer<T> _comparer;

    public NonGenericComparerAdapter(IComparer<T>? comparer = null)
    {
        _comparer = comparer ?? Comparer<T>.Default;
    }

    public int Compare(object? x, object? y)
    {
        return ComparerHelper.TryCompare(x, y, out var result)
            ? result.Value
            : _comparer.Compare((T)x, (T)y);
    }
}