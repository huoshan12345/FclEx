namespace FclEx.Comparers;

public class CommonEqualityComparer
{
    public static CommonEqualityComparer<T> Create<T>(Func<T, T, bool> compareFunc, Func<T, int> hashFunc)
    {
        return new(compareFunc, hashFunc);
    }
}

public class CommonEqualityComparer<T> : IEqualityComparer<T>
{
    private readonly Func<T, T, bool> _compareFunc;
    private readonly Func<T, int> _hashFunc;

    public CommonEqualityComparer(Func<T, T, bool> compareFunc, Func<T, int> hashFunc)
    {
        _compareFunc = compareFunc ?? throw new ArgumentNullException(nameof(compareFunc));
        _hashFunc = hashFunc ?? throw new ArgumentNullException(nameof(hashFunc));
    }

    public bool Equals(T? x, T? y)
    {
        if (ReferenceEquals(x, y))
            return true;

        if (x == null || y == null)
            return false;

        return _compareFunc(x, y);
    }

    public int GetHashCode(T obj)
    {
        return _hashFunc(obj);
    }
}