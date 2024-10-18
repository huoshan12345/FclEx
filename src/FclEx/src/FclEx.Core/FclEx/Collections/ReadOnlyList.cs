namespace FclEx.Collections;

[CollectionBuilder(typeof(ReadOnlyListBuilder), nameof(ReadOnlyListBuilder.Create))]
public class ReadOnlyList<T> : IReadOnlyList<T>
{
    private readonly IReadOnlyList<T> _list;

    public ReadOnlyList(IReadOnlyList<T>? list = null)
    {
        _list = list ?? [];
    }

    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public int Count => _list.Count;
    public T this[int index] => _list[index];

    public static implicit operator ReadOnlyList<T>(List<T> list) => new(list);
    public static implicit operator ReadOnlyList<T>(T[] array) => new(array);

    public override string ToString()
    {
        using var builder = new ValueStringBuilder();
        builder.Append('[');
        foreach (var (_, item, _, isLast) in _list.IndexExt())
        {
            if (item is null)
                continue;

            builder.Append(item.ToString());
            if (isLast == false)
                builder.Append(", ");
        }
        builder.Append(']');
        return builder.ToString();
    }
}

internal static class ReadOnlyListBuilder
{
    internal static ReadOnlyList<T> Create<T>(ReadOnlySpan<T> values) => new(values.ToArray());
}

public static class ReadOnlyListExtensions
{
    public static ReadOnlyList<T> ToReadOnlyList<T>(this IEnumerable<T> enumerable)
    {
        return new ReadOnlyList<T>(enumerable.AsIReadOnlyList());
    }
}