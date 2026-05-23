namespace System.Collections.Generic;

public static class EnumerableComparer
{
    public static IComparer<IEnumerable<T>> Default<T>() => EnumerableComparer<T>.Default;
    public static readonly IComparer<IEnumerable<string>> String = EnumerableComparer<string>.Default;
    public static readonly IComparer<IEnumerable<string>> StringOrdinalIgnoreCase = new EnumerableComparer<string>(StringComparer.OrdinalIgnoreCase);

    public static EnumerableComparer<T> Create<T>(IComparer<T>? itemComparer) => new(itemComparer);
}

public class EnumerableComparer<T>(IComparer<T>? itemComparer = null) : IComparer<IEnumerable<T>>
{
    public static readonly EnumerableComparer<T> Default = new();

    private readonly IComparer<T> _itemComparer = itemComparer ?? Comparer<T>.Default;

    public int Compare(IEnumerable<T>? x, IEnumerable<T>? y)
    {
        if (ComparerHelper.TryCompare(x, y, out var result))
            return result.Value;

        using var e1 = x.GetEnumerator();
        using var e2 = y.GetEnumerator();

        while (true)
        {
            var b1 = e1.MoveNext();
            var b2 = e2.MoveNext();

            switch (b1, b2)
            {
                case (false, false): return 0;
                case (true, false): return 1;
                case (false, true): return -1;
            }

            var m1 = e1.Current;
            var m2 = e2.Current;

            var compare = _itemComparer.Compare(m1, m2);
            if (compare != 0)
                return compare;
        }
    }
}