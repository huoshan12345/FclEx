namespace FclEx.Extensions;

partial class EnumerableExtensions
{
    /// <summary>
    /// Sorts the elements of a string sequence in ascending order using
    /// ordinal (byte-value) comparison, avoiding culture-sensitive sorting.
    /// </summary>
    public static IOrderedEnumerable<string> OrderByOrdinal(this IEnumerable<string> source)
    {
        return source.OrderBy(s => s, StringComparer.Ordinal);
    }

    /// <summary>
    /// Sorts the elements of a string sequence in descending order using
    /// ordinal (byte-value) comparison, avoiding culture-sensitive sorting.
    /// </summary>
    public static IOrderedEnumerable<string> OrderByDescendingOrdinal(this IEnumerable<string> source)
    {
        return source.OrderByDescending(s => s, StringComparer.Ordinal);
    }

    /// <summary>
    /// Performs a subsequent ascending ordinal sort on an already-ordered
    /// string sequence. Returns IOrderedEnumerable so it can be chained
    /// with further ThenBy calls.
    /// </summary>
    public static IOrderedEnumerable<string> ThenByOrdinal(this IOrderedEnumerable<string> source)
    {
        return source.ThenBy(s => s, StringComparer.Ordinal);
    }

    /// <summary>
    /// Performs a subsequent descending ordinal sort on an already-ordered
    /// string sequence. Returns IOrderedEnumerable so it can be chained
    /// with further ThenBy calls.
    /// </summary>
    public static IOrderedEnumerable<string> ThenByDescendingOrdinal(this IOrderedEnumerable<string> source)
    {
        return source.ThenByDescending(s => s, StringComparer.Ordinal);
    }
    
    /// <summary>
    /// Sorts the elements of a sequence in ascending order according to a
    /// string key, using ordinal (byte-value) comparison.
    /// </summary>
    public static IOrderedEnumerable<T> OrderByOrdinal<T>(this IEnumerable<T> source, Func<T, string> keySelector)
    {
        return source.OrderBy(keySelector, StringComparer.Ordinal);
    }

    /// <summary>
    /// Sorts the elements of a sequence in descending order according to a
    /// string key, using ordinal (byte-value) comparison.
    /// </summary>
    public static IOrderedEnumerable<T> OrderByDescendingOrdinal<T>(this IEnumerable<T> source, Func<T, string> keySelector)
    {
        return source.OrderByDescending(keySelector, StringComparer.Ordinal);
    }

    /// <summary>
    /// Performs a subsequent ascending ordinal sort on an already-ordered
    /// sequence according to a string key. Returns IOrderedEnumerable so
    /// it can be chained with further ThenBy calls.
    /// </summary>
    public static IOrderedEnumerable<T> ThenByOrdinal<T>(this IOrderedEnumerable<T> source, Func<T, string> keySelector)
    {
        return source.ThenBy(keySelector, StringComparer.Ordinal);
    }

    /// <summary>
    /// Performs a subsequent descending ordinal sort on an already-ordered
    /// sequence according to a string key. Returns IOrderedEnumerable so
    /// it can be chained with further ThenBy calls.
    /// </summary>
    public static IOrderedEnumerable<T> ThenByDescendingOrdinal<T>(this IOrderedEnumerable<T> source, Func<T, string> keySelector)
    {
        return source.ThenByDescending(keySelector, StringComparer.Ordinal);
    }
}
