namespace System.Collections.Generic;

[CollectionBuilder(typeof(ReadOnlySetBuilder), nameof(ReadOnlySetBuilder.Create))]
public class ReadOnlySet<T>(ISet<T>? set = null) : IReadOnlyContainer<T>
{
    private readonly ISet<T> _set = set ?? new HashSet<T>();

    public int Count => _set.Count;
    public bool Contains(T item) => _set.Contains(item);
    public IEnumerator<T> GetEnumerator() => _set.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

internal static class ReadOnlySetBuilder
{
    internal static ReadOnlySet<T> Create<T>(ReadOnlySpan<T> values) => new(values.ToHashSet());
}

public static class ReadOnlySetExtensions
{
    public static ReadOnlySet<T> ToReadOnlySet<T>(this IEnumerable<T> enumerable)
    {
        return new ReadOnlySet<T>(enumerable.AsISet());
    }
}