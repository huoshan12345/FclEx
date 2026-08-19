namespace FclEx.Extensions;

public static partial class EnumerableExtensions
{
    /// <summary>
    /// Filters out the null values from a collection of nullable elements.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="enumerable">The collection of nullable elements to filter.</param>
    /// <returns>An IEnumerable containing the non-null elements of the collection.</returns>
    /// <remarks>
    /// This method uses <see cref="MethodImplOptions.AggressiveInlining"/> for potential performance optimization.
    /// </remarks>
    [MethodImpl(AggressiveInlining)]
    public static IEnumerable<T> NotNull<T>(this IEnumerable<T?> enumerable)
        => enumerable.Where(m => m is not null)!;

    /// <summary>
    /// Filters out the null values from a collection of nullable value types and unboxes them to non-nullable types.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection. Must be a value type.</typeparam>
    /// <param name="enumerable">The collection of nullable value types to filter.</param>
    /// <returns>An IEnumerable containing the non-null elements of the collection, unboxed to their non-nullable type.</returns>
    /// <remarks>
    /// This method uses <see cref="MethodImplOptions.AggressiveInlining"/> for potential performance optimization.
    /// </remarks>
    [MethodImpl(AggressiveInlining)]
    public static IEnumerable<T> NotNull<T>(this IEnumerable<T?> enumerable) where T : struct
        => enumerable.Where(m => m is not null).Select(m => m.Get());

    /// <summary>
    /// Filters the elements of a collection to exclude those that satisfy the given predicate.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="enumerable">The collection of elements to filter.</param>
    /// <param name="predicate">The predicate function that defines the condition to exclude.</param>
    /// <returns>An IEnumerable containing elements that do not satisfy the predicate.</returns>
    [MethodImpl(AggressiveInlining)]
    public static IEnumerable<T> Not<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
        => enumerable.Where(m => predicate(m) == false);

    [MethodImpl(AggressiveInlining)]
    public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate, bool condition)
        => condition ? enumerable.Where(predicate) : enumerable;

    [MethodImpl(AggressiveInlining)]
    public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> enumerable, bool condition, Func<T, bool> predicate)
        => enumerable.WhereIf(predicate, condition);

    [MethodImpl(AggressiveInlining)]
    public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> enumerable, Func<T, int, bool> predicate, bool condition)
        => condition ? enumerable.Where(predicate) : enumerable;

    [MethodImpl(AggressiveInlining)]
    public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> enumerable, bool condition, Func<T, int, bool> predicate)
        => enumerable.WhereIf(predicate, condition);

    [MethodImpl(AggressiveInlining)]
    public static IEnumerable<T> TryTake<T>(this IEnumerable<T> enumerable, int? count)
    {
        return count is { } c ? enumerable.Take(c) : enumerable;
    }

    [MethodImpl(AggressiveInlining)]
    public static IEnumerable<T> TryWhere<T>(this IEnumerable<T> enumerable, Func<T, bool>? predicate)
    {
        return predicate != null ? enumerable.Where(predicate) : enumerable;
    }

    [MethodImpl(AggressiveInlining)]
    public static IEnumerable<T> TryWhere<T>(this IEnumerable<T> enumerable, Func<T, int, bool>? predicate)
    {
        return predicate != null ? enumerable.Where(predicate) : enumerable;
    }

    public static IEnumerable<T> Except<T>(this IEnumerable<T> enumerable, T item, IEqualityComparer<T>? comparer = null)
    {
        comparer ??= EqualityComparer<T>.Default;
        return enumerable.Where(m => !comparer.Equals(m, item));
    }
}
