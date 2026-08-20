namespace FclEx.Extensions;

partial class EnumerableExtensions
{
    /// <summary>Determines whether the sequence contains any of the specified values.</summary>
    /// <remarks>
    /// <paramref name="enumerable"/> is materialized into a <see cref="HashSet{T}"/> rather than
    /// <paramref name="values"/>, so that each element of <paramref name="values"/> can be looked up
    /// in O(1) instead of doing an O(n) scan per value. This also means <paramref name="values"/> is
    /// only enumerated once, so it may safely be a one-shot / deferred sequence; <paramref name="enumerable"/>
    /// is fully enumerated regardless of whether a match is found early.
    /// </remarks>
    public static bool ContainsAny<T>(this IEnumerable<T> enumerable, IEnumerable<T> values, IEqualityComparer<T>? comparer = null)
    {
        Check.NotNull(values);
        var set = new HashSet<T>(enumerable, comparer ?? EqualityComparer<T>.Default);
        return values.Any(set.Contains);
    }

    public static bool ContainsAll<T>(this IEnumerable<T> enumerable, IEnumerable<T> values, IEqualityComparer<T>? comparer = null)
    {
        Check.NotNull(values);
        var set = new HashSet<T>(enumerable, comparer ?? EqualityComparer<T>.Default);
        return values.All(set.Contains);
    }
}
