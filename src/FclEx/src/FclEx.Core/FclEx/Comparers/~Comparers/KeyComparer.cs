namespace FclEx.Comparers;

public class KeyComparer
{
    public static KeyComparer<T, TKey> Create<T, TKey>(Func<T, TKey?> keySelector, IComparer<TKey?>? keyComparer = null, bool isNullSmaller = true)
    {
        return new(keySelector, keyComparer, isNullSmaller);
    }
}

public class KeyComparer<T>
{
    public static KeyComparer<T, TKey> Create<TKey>(Func<T, TKey?> keySelector, IComparer<TKey?>? keyComparer = null, bool isNullSmaller = true)
    {
        return new(keySelector, keyComparer, isNullSmaller);
    }
}

public class KeyComparer<T, TKey> : IComparer<T>
{
    private readonly IComparer<T> _comparer;

    public KeyComparer(Func<T, TKey?> keySelector, IComparer<TKey?>? keyComparer = null, bool isNullSmaller = true)
    {
        _comparer = MemberComparerBuilder<T>.Create(keySelector, false, keyComparer, isNullSmaller).Build();
    }

    public int Compare(T? x, T? y)
    {
        return _comparer.Compare(x, y);
    }
}