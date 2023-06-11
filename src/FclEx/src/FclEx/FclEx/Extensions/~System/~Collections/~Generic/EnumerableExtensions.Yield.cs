using System.Collections.Generic;

namespace FclEx.Extensions;

partial class EnumerableExtensions
{
    /// <summary>
    /// Wraps this object instance into an IEnumerable
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> Yield<T>(this T item)
    {
        yield return item;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> Yield<T>(this (T, T) items)
    {
        yield return items.Item1;
        yield return items.Item2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> Yield<T>(this (T, T, T) items)
    {
        yield return items.Item1;
        yield return items.Item2;
        yield return items.Item3;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> Yield<T>(this (T, T, T, T) items)
    {
        yield return items.Item1;
        yield return items.Item2;
        yield return items.Item3;
        yield return items.Item4;
    }
}