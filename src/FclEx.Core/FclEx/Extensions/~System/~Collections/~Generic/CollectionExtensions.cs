namespace FclEx.Extensions;

public static class CollectionExtensions
{
    public static bool AddIfNotNull<T>(this ICollection<T> collection, [NotNullWhen(true)] T? item)
    {
        Check.NotNull(collection);
        if (item is not null)
        {
            collection.Add(item);
            return true;
        }
        return false;
    }

    public static void AddRangeSafely<T>(this ICollection<T> collection, IEnumerable<T>? items)
    {
        Check.NotNull(collection);

        if (items == null)
            return;

        if (collection is List<T> list)
        {
            list.AddRange(items);
        }
        else
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }
    }

    public static int CountSafely<T>(this ICollection<T>? col)
    {
        return col?.Count ?? 0;
    }

    public static TCollection Push<T, TCollection>(this TCollection col, T item) where TCollection : ICollection<T>
    {
        col.Add(item);
        return col;
    }
}