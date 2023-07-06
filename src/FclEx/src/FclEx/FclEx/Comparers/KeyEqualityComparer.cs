namespace FclEx.Comparers;

public class KeyEqualityComparer
{
    public static KeyEqualityComparer<T, TKey> Create<T, TKey>(Func<T, TKey> keySelector, IEqualityComparer<TKey>? keyComparer = null)
    {
        return new(keySelector, keyComparer);
    }
}

public class KeyEqualityComparer<T>
{
    public static KeyEqualityComparer<T, TKey> Create<TKey>(Func<T, TKey> keySelector, IEqualityComparer<TKey>? keyComparer = null)
    {
        return new(keySelector, keyComparer);
    }
}

public class KeyEqualityComparer<T, TKey> : IEqualityComparer<T>
{
    private readonly Func<T, TKey> _keySelector;
    private readonly IEqualityComparer<TKey> _keyComparer;

    public KeyEqualityComparer(Func<T, TKey> keySelector, IEqualityComparer<TKey>? keyComparer = null)
    {
        _keySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
        _keyComparer = keyComparer ?? EqualityComparer<TKey>.Default;
    }

    public bool Equals(T? x, T? y)
    {
        if (ReferenceEquals(x, y))
            return true;

        if (x == null || y == null)
            return false;

        return _keyComparer.Equals(_keySelector(x), _keySelector(y));
    }

    public int GetHashCode(T obj)
    {
        var value = _keySelector(obj);
        return value == null
            ? 0
            : _keyComparer.GetHashCode(value);
    }
}