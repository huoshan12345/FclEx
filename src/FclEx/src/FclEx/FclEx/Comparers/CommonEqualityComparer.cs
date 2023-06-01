using System.Collections.Generic;

namespace FclEx.Comparers;

internal class CommonEqualityComparer<T> : IEqualityComparer<T>
{
    private readonly Func<T?, T?, bool> _compareFunc;
    private readonly Func<T, int> _hashFunc;

    public CommonEqualityComparer(Func<T?, T?, bool> compareFunc, Func<T, int>? hashFunc = null)
    {
        _compareFunc = compareFunc ?? throw new ArgumentNullException(nameof(compareFunc));
        _hashFunc = hashFunc ?? (x => x.GetHashCodeSafely());
    }

    public bool Equals(T? x, T? y)
    {
        return _compareFunc(x, y);
    }

    public int GetHashCode(T obj)
    {
        return _hashFunc(obj);
    }
}