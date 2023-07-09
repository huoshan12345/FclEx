namespace FclEx.Extensions;

public static class CollectionExtensions
{
    public static ICollection<T> AddIfNotNull<T>(this ICollection<T> source, T? item)
    {
        Check.NotNull(source);
        if (item is not null)
            source.Add(item);
        return source;
    }

    public static void AddRangeSafely<T>(this ICollection<T> col, IEnumerable<T>? items)
    {
        if (items == null)
            return;

        if (col is List<T> list)
        {
            list.AddRange(items);
        }
        else
        {
            foreach (var item in items)
            {
                col.Add(item);
            }
        }
    }
}