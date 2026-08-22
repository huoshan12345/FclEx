namespace System.Collections.Generic;

public class DelegateComparer
{
    public static DelegateComparer<T> Create<T>(Comparison<T> comparison)
    {
        return new(comparison);
    }
}

public class DelegateComparer<T> : IComparer<T>
{
    private readonly Comparison<T> _comparison;

    public DelegateComparer(Comparison<T> comparison)
    {
        _comparison = comparison ?? throw new ArgumentNullException(nameof(comparison));
    }

    public int Compare(T? x, T? y)
    {
        return Comparer.TryCompare(x, y, out var result)
            ? result.Value
            : _comparison(x, y);
    }
}
