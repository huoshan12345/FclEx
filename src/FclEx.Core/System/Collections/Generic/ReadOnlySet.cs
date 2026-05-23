namespace System.Collections.Generic;

// NOTE: The ReadOnlySet from .Net 9.0 does not support collection initializer,
// so we implement our own version here and do not put it in the same namespace.
[CollectionBuilder(typeof(ReadOnlySetBuilder), nameof(ReadOnlySetBuilder.Create))]
public class ReadOnlyHashSet<T>(ISet<T>? set = null) : IReadOnlyContainer<T>
{
    private readonly ISet<T> _set = set ?? new HashSet<T>();

    public int Count => _set.Count;
    public bool Contains(T item) => _set.Contains(item);
    public IEnumerator<T> GetEnumerator() => _set.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

internal static class ReadOnlySetBuilder
{
    internal static ReadOnlyHashSet<T> Create<T>(ReadOnlySpan<T> values) => new(values.ToHashSet());
}

public static class ReadOnlySetExtensions
{
    public static ReadOnlyHashSet<T> ToReadOnlySet<T>(this IEnumerable<T> enumerable)
    {
        return new ReadOnlyHashSet<T>(enumerable.AsISet());
    }
}