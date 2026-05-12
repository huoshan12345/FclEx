namespace FclEx.Extensions;

public static class SetExtensions
{
    public static void AddRange<T>(this ISet<T> set, IEnumerable<T> items)
    {
        foreach (var item in items)
            set.Add(item);
    }

#if !NET5_0_OR_GREATER
    /// <summary>
    /// Returns a read-only <see cref="ReadOnlySet{T}"/> wrapper
    /// for the specified set.
    /// </summary>
    /// <typeparam name="T">The type of elements in the set.</typeparam>
    /// <param name="set">The set to wrap.</param>
    /// <returns>An object that acts as a read-only wrapper around the current <see cref="ISet{T}"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="set"/> is null.</exception>
    public static ReadOnlyHashSet<T> AsReadOnly<T>(this ISet<T> set) => new(set);
#endif
}