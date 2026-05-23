namespace System.Collections.Generic;

public class ReverseComparer<T> : IComparer<T>
{
    private readonly IComparer<T> _comparer;

    public ReverseComparer(IComparer<T>? comparer = null)
    {
        _comparer = comparer ?? Comparer<T>.Default;
    }

    public int Compare(T? x, T? y)
    {
        return _comparer.Compare(y!, x!);
    }
}