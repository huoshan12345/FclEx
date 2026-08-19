namespace FclEx.Extensions;

public static partial class EnumerableExtensions
{
    /// <summary>
    /// Interleaves elements from two sequences by alternately yielding groups of
    /// elements from each sequence.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequences.</typeparam>
    /// <param name="first">The first source sequence.</param>
    /// <param name="second">The second source sequence.</param>
    /// <param name="firstGrouping">
    /// The number of consecutive elements to take from <paramref name="first"/> at a time.
    /// </param>
    /// <param name="secondGrouping">
    /// The number of consecutive elements to take from <paramref name="second"/> at a time.
    /// </param>
    /// <returns>
    /// A sequence that yields <paramref name="firstGrouping"/> elements from
    /// <paramref name="first"/>, followed by <paramref name="secondGrouping"/> elements
    /// from <paramref name="second"/>, repeating until both sequences are exhausted.
    /// </returns>
    /// <remarks>
    /// Enumeration is lazy. If one sequence runs out of elements, the remaining
    /// elements from the other sequence are yielded.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="first"/> or <paramref name="second"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if <paramref name="firstGrouping"/> or <paramref name="secondGrouping"/> is less than or equal to zero.
    /// </exception>
    public static IEnumerable<T> InterleaveWith<T>(this IEnumerable<T> first, IEnumerable<T> second, int firstGrouping, int secondGrouping)
    {
        Check.NotNull(first);
        Check.NotNull(second);
        Check.Positive(firstGrouping);
        Check.Positive(secondGrouping);
        return InterleaveWithIterator(first, second, firstGrouping, secondGrouping);

        static IEnumerable<T> InterleaveWithIterator(IEnumerable<T> first, IEnumerable<T> second, int firstGrouping, int secondGrouping)
        {
            using var firstIterator = first.GetEnumerator();
            using var secondIterator = second.GetEnumerator();
            var exhaustedFirst = false;
            // Keep going while we've got elements in the first sequence.
            while (!exhaustedFirst)
            {
                for (var i = 0; i < firstGrouping; i++)
                {
                    if (!firstIterator.MoveNext())
                    {
                        exhaustedFirst = true;
                        break;
                    }
                    yield return firstIterator.Current;
                }
                // This may not yield any results - the first sequence
                // could go on for much longer than the second. It does no
                // harm though; we can keep calling MoveNext() as often
                // as we want.
                for (var i = 0; i < secondGrouping; i++)
                {
                    // This is a bit ugly, but it works...
                    if (!secondIterator.MoveNext())
                    {
                        break;
                    }
                    yield return secondIterator.Current;
                }
            }
            // We may have elements in the second sequence left over.
            // Yield them all now.
            while (secondIterator.MoveNext())
            {
                yield return secondIterator.Current;
            }
        }
    }
}
