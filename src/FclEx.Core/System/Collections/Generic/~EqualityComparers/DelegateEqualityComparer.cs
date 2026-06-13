namespace System.Collections.Generic;

public class DelegateEqualityComparer
{
    public static DelegateEqualityComparer<T> Create<T>(Func<T, T, bool> compareFunc, Func<T, int> hashFunc)
    {
        return new(compareFunc, hashFunc);
    }
}

public class DelegateEqualityComparer<T> : IEqualityComparer<T>
{
    private readonly Func<T, T, bool> _compareFunc;
    private readonly Func<T, int> _hashFunc;

    public DelegateEqualityComparer(Func<T, T, bool> compareFunc, Func<T, int> hashFunc)
    {
        _compareFunc = compareFunc ?? throw new ArgumentNullException(nameof(compareFunc));
        _hashFunc = hashFunc ?? throw new ArgumentNullException(nameof(hashFunc));
    }

    public bool Equals(T? x, T? y)
    {
        return ComparerHelper.TryEquals(x, y, out var result) 
            ? result.Value : 
            _compareFunc(x, y);
    }

    public int GetHashCode(T obj)
    {
        return _hashFunc(obj);
    }
}
