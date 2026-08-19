namespace FclEx.Extensions;

public static partial class EnumerableExtensions
{
    /// <summary>
    /// Searches for the first element that satisfies the specified predicate.
    /// </summary>
    /// <typeparam name="T">The sequence element type.</typeparam>
    /// <param name="source">The sequence to search.</param>
    /// <param name="match">The predicate that defines the element to find.</param>
    /// <returns>The zero-based index of the first matching element, or -1 when no element matches.</returns>
    public static int FindIndex<T>(this IEnumerable<T> source, Predicate<T> match)
    {
        Check.NotNull(source);
        Check.NotNull(match);

        return source switch
        {
            List<T> list => list.FindIndex(match),
            T[] array => Array.FindIndex(array, match),
            _ => FindIndexCore(source, match),
        };

        static int FindIndexCore(IEnumerable<T> source, Predicate<T> match)
        {
            var index = 0;
            foreach (var item in source)
            {
                if (match(item))
                    return index;

                index++;
            }
            return -1;
        }
    }

    /// <summary>
    /// Searches for the first element that satisfies the specified predicate, starting at a given index.
    /// </summary>
    /// <typeparam name="T">The sequence element type.</typeparam>
    /// <param name="source">The sequence to search.</param>
    /// <param name="startIndex">The zero-based starting index of the search.</param>
    /// <param name="match">The predicate that defines the element to find.</param>
    /// <returns>The zero-based index of the first matching element, or -1 when no element matches.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="startIndex"/> is negative or greater than the sequence length.</exception>
    public static int FindIndex<T>(this IEnumerable<T> source, int startIndex, Predicate<T> match)
    {
        Check.NotNull(source);
        Check.NotNull(match);
        Check.NotNegative(startIndex);

        return source switch
        {
            List<T> list => list.FindIndex(startIndex, match),
            T[] array => Array.FindIndex(array, startIndex, match),
            _ => FindIndexCore(source, startIndex, match),
        };

        static int FindIndexCore(IEnumerable<T> source, int startIndex, Predicate<T> match)
        {
            var index = 0;
            foreach (var item in source)
            {
                if (index >= startIndex && match(item))
                    return index;

                index++;
            }

            if (startIndex > index)
                throw new ArgumentOutOfRangeException(nameof(startIndex), startIndex, $"The value must be between 0 and {index}.");

            return -1;
        }
    }

    /// <summary>
    /// Searches a range for the first element that satisfies the specified predicate.
    /// </summary>
    /// <typeparam name="T">The sequence element type.</typeparam>
    /// <param name="source">The sequence to search.</param>
    /// <param name="startIndex">The zero-based starting index of the search range.</param>
    /// <param name="count">The number of elements in the search range.</param>
    /// <param name="match">The predicate that defines the element to find.</param>
    /// <returns>The zero-based index of the first matching element, or -1 when no element matches.</returns>
    /// <remarks>
    /// For non-countable sequences, the method enumerates only enough elements to validate the requested range and search it.
    /// The predicate is not invoked when the requested range is invalid.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="startIndex"/> or <paramref name="count"/> is negative,
    /// or when the requested range extends past the sequence length.
    /// </exception>
    public static int FindIndex<T>(this IEnumerable<T> source, int startIndex, int count, Predicate<T> match)
    {
        Check.NotNull(source);
        Check.NotNull(match);
        Check.NotNegative(startIndex);
        Check.NotNegative(count);

        return source switch
        {
            List<T> list => list.FindIndex(startIndex, count, match),
            T[] array => Array.FindIndex(array, startIndex, count, match),
            _ => FindIndexCore(source, startIndex, count, match),
        };

        static int FindIndexCore(IEnumerable<T> source, int startIndex, int count, Predicate<T> match)
        {
            if (source.TryGetNonEnumeratedCount(out var sourceCount))
            {
                EnsureValidRange(startIndex, count, sourceCount);
                return FindIndexInRange(source, startIndex, count, match);
            }

            return FindIndexInRangeWithUnknownCount(source, startIndex, count, match);
        }

        static int FindIndexInRange(IEnumerable<T> source, int startIndex, int count, Predicate<T> match)
        {
            var endIndex = startIndex + count;
            var index = 0;
            foreach (var item in source)
            {
                if (index >= endIndex)
                    return -1;

                if (index >= startIndex && match(item))
                    return index;

                index++;
            }

            return -1;
        }

        static int FindIndexInRangeWithUnknownCount(IEnumerable<T> source, int startIndex, int count, Predicate<T> match)
        {
            var candidates = new List<T>();
            using var enumerator = source.GetEnumerator();

            var index = 0;
            while (index < startIndex)
            {
                if (enumerator.MoveNext() == false)
                    throw new ArgumentOutOfRangeException(nameof(startIndex), startIndex, $"The value must be between 0 and {index}.");

                index++;
            }

            for (var i = 0; i < count; i++)
            {
                if (enumerator.MoveNext() == false)
                    throw new ArgumentOutOfRangeException(nameof(count), count, $"The value must be between 0 and {i}.");

                candidates.Add(enumerator.Current);
            }

            for (var i = 0; i < candidates.Count; i++)
            {
                if (match(candidates[i]))
                    return startIndex + i;
            }

            return -1;
        }

        static void EnsureValidRange(int startIndex, int count, int sourceCount)
        {
            if (startIndex > sourceCount)
                throw new ArgumentOutOfRangeException(nameof(startIndex), startIndex, $"The value must be between 0 and {sourceCount}.");

            if (count > sourceCount - startIndex)
                throw new ArgumentOutOfRangeException(nameof(count), count, $"The value must be between 0 and {sourceCount - startIndex}.");
        }
    }
}
