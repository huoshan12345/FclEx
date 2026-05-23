namespace System.Collections.Generic;

public static class EnumerableEqualityComparer
{
    public static IEqualityComparer<IEnumerable<T>> Default<T>() => EnumerableEqualityComparer<T>.Default;
    public static readonly IEqualityComparer<IEnumerable<string>> String = EnumerableEqualityComparer<string>.Default;
    public static readonly IEqualityComparer<IEnumerable<string>> StringOrdinalIgnoreCase = new EnumerableEqualityComparer<string>(StringComparer.OrdinalIgnoreCase);

    public static IEqualityComparer<IEnumerable<T>> Create<T>(IEqualityComparer<T>? itemComparer = null)
    {
        return new EnumerableEqualityComparer<T>(itemComparer);
    }
}

public class EnumerableEqualityComparer<T>(IEqualityComparer<T>? itemComparer = null) : IEqualityComparer<IEnumerable<T>>
{
    public static readonly EnumerableEqualityComparer<T> Default = new();

    private readonly IEqualityComparer<T> _itemComparer = itemComparer ?? EqualityComparer<T>.Default;

    public bool Equals(IEnumerable<T>? x, IEnumerable<T>? y)
    {
        if (ComparerHelper.TryEquals(x, y, out var result))
            return result.Value;

        if (x.GetType() != y.GetType())
            return false;

#if NET6_0_OR_GREATER
        if (x.TryGetNonEnumeratedCount(out var count1)
            && y.TryGetNonEnumeratedCount(out var count2))
        {
            if (count1 != count2)
                return false;

            if (count1 == 0)
                return true;
        }        
#endif
        using var e1 = x.GetEnumerator();
        using var e2 = y.GetEnumerator();

        while (true)
        {
            var hasNext = e1.MoveNext();
            if (hasNext != e2.MoveNext())
                return false;

            if (hasNext == false)
                break;

            if (_itemComparer.Equals(e1.Current, e2.Current) == false)
                return false;
        }

        return true;
    }

    public int GetHashCode(IEnumerable<T> obj)
    {
        var hash = new HashCode();
        foreach (var m in obj)
        {
            hash.Add(m);
        }
        return hash.ToHashCode();
    }
}