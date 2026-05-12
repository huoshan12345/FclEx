using System.Collections.ObjectModel;

namespace FclEx.Extensions;

[SuppressMessage("ReSharper", "MoveToExtensionBlock")]
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

#if !NET5_0_OR_GREATER
    /// <summary>
    /// Returns a read-only <see cref="ReadOnlyCollection{T}"/> wrapper
    /// for the specified list.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="list">The list to wrap.</param>
    /// <returns>An object that acts as a read-only wrapper around the current <see cref="IList{T}"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="list"/> is null.</exception>
    public static ReadOnlyCollection<T> AsReadOnly<T>(this IList<T> list) => new(list);

    /// <summary>
    /// Returns a read-only <see cref="ReadOnlySet{T}"/> wrapper
    /// for the specified set.
    /// </summary>
    /// <typeparam name="T">The type of elements in the set.</typeparam>
    /// <param name="set">The set to wrap.</param>
    /// <returns>An object that acts as a read-only wrapper around the current <see cref="ISet{T}"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="set"/> is null.</exception>
    public static ReadOnlySet<T> AsReadOnly<T>(this ISet<T> set) => new(set);

    public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
    {
        Check.NotNull(dictionary);

        if (dictionary.ContainsKey(key)) 
            return false;

        dictionary.Add(key, value);
        return true;
    }
#endif
}