namespace System.Collections.Generic;

public class CommonComparer
{
    public static CommonComparer<T> Create<T>(Comparison<T> comparison)
    {
        return new(comparison);
    }
}

public class CommonComparer<T> : IComparer<T>
{
    private readonly Comparison<T> _comparison;

    public CommonComparer(Comparison<T> comparison)
    {
        _comparison = comparison ?? throw new ArgumentNullException(nameof(comparison));
    }

    public int Compare(T? x, T? y)
    {
        return ComparerHelper.TryCompare(x, y, out var result)
            ? result.Value
            : _comparison(x, y);
    }
}