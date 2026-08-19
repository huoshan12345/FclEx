namespace FclEx.Extensions;

public static partial class EnumerableExtensions
{
    [MethodImpl(AggressiveInlining)]
    public static string JoinWith<T>(this IEnumerable<T> enumerable, string? separator)
    {
        return StringBuilderHelper.Build(m => m.AppendJoin(separator, enumerable));
    }

    public static SortedSet<T> ToSortedSet<T>(this IEnumerable<T> enumerable, IComparer<T>? comparer = null)
    {
        return new SortedSet<T>(enumerable, comparer ?? Comparer<T>.Default);
    }

    /// <summary>
    /// Converts up to 32 Boolean values to the corresponding signed 32-bit bit pattern.
    /// </summary>
    /// <remarks>
    /// The first value is the least-significant bit. When the 32nd value is set, the returned value is negative because
    /// that position is the sign bit of <see cref="int"/>.
    /// </remarks>
    public static int BitsToInt(this IEnumerable<bool> bits)
    {
        Check.NotNull(bits);

        var num = 0;
        var index = 0;
        foreach (var bit in bits)
        {
            if (index == 32)
                throw new ArgumentException("The sequence must contain at most 32 bits.", nameof(bits));

            if (bit)
                num |= 1 << index;

            index++;
        }
        return num;
    }

#if !NET5_0_OR_GREATER && !NET472_OR_GREATER
    /// <summary>Creates a <see cref="T:System.Collections.Generic.HashSet`1" /> from an <see cref="T:System.Collections.Generic.IEnumerable`1" /> using the <paramref name="comparer" /> to compare keys.</summary>
    /// <param name="source">An <see cref="T:System.Collections.Generic.IEnumerable`1" /> to create a <see cref="T:System.Collections.Generic.HashSet`1" /> from.</param>
    /// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to compare keys.</param>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
    /// <returns>A <see cref="T:System.Collections.Generic.HashSet`1" /> that contains values of type <typeparamref name="TSource" /> selected from the input sequence.</returns>
    public static HashSet<TSource> ToHashSet<TSource>(this IEnumerable<TSource> source, IEqualityComparer<TSource>? comparer = null)
    {
        Check.NotNull(source);
        return new HashSet<TSource>(source, comparer);
    }
#endif
}
