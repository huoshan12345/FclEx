namespace System.Collections.Generic;

public class CommonComparer
{
    public static CommonComparer<T> Create<T>(Comparison<T> comparison, bool isNullSmaller = true)
    {
        return new(comparison, isNullSmaller);
    }
}

public class CommonComparer<T> : IComparer<T>
{
    private readonly Comparison<T> _comparison;
    private readonly bool _isNullSmaller;

    public CommonComparer(Comparison<T> comparison, bool isNullSmaller = true)
    {
        _comparison = comparison ?? throw new ArgumentNullException(nameof(comparison));
        _isNullSmaller = isNullSmaller;
    }

    public int Compare(T? x, T? y)
    {
        return ComparerHelper.TryCompare(x, y, out var result)
            ? result.Value
            : _comparison(x, y);
    }
}