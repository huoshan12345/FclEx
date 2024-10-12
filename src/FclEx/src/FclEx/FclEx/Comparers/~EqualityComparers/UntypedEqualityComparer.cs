using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FclEx.Comparers;

public class UntypedEqualityComparer
{
    public static UntypedEqualityComparer<T> Create<T>(IEqualityComparer<T> comparer) => new(comparer);
}

public class UntypedEqualityComparer<T> : IEqualityComparer
{
    private readonly IEqualityComparer<T> _comparer;

    public UntypedEqualityComparer(IEqualityComparer<T> comparer)
    {
        _comparer = comparer;
    }

    public new bool Equals(object? x, object? y)
    {
        return ComparerHelper.TryEquals(x, y, out var result)
            ? result.Value
            : _comparer.Equals((T)x, (T)y);
    }

    public int GetHashCode(object obj)
    {
        return _comparer.GetHashCode(obj.CastTo<T>());
    }
}