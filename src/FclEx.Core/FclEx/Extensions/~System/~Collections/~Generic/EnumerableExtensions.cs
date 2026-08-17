namespace FclEx.Extensions;

public readonly record struct IndexedItem<T>(int Index, T Item, bool IsFirst, bool IsLast);

public static partial class EnumerableExtensions
{
    [MethodImpl(AggressiveInlining)]
    public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this IEnumerable<T>? source) => source is null || source.AnyEx() == false;

    [MethodImpl(AggressiveInlining)]
    public static bool IsEmpty<T>(this IEnumerable<T> source) => source.AnyEx() == false;

    [MethodImpl(AggressiveInlining)]
    public static bool IsNotEmpty<T>([NotNullWhen(true)] this IEnumerable<T>? enumerable) => enumerable.IsNullOrEmpty() == false;

    [MethodImpl(AggressiveInlining)]
    public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? source) => source ?? [];

    [MethodImpl(AggressiveInlining)]
    public static string JoinWith<T>(this IEnumerable<T> enumerable, string? separator)
    {
        return StringBuilderHelper.Build(m => m.AppendJoin(separator, enumerable));
    }

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

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public static IEnumerable<TResult> SelectMany<T, TResult>(this IEnumerable<T> source, Func<T, T, TResult> resultSelector)
    {
        return source.SelectMany(m => source, resultSelector);
    }

    public static IEnumerable<T> Except<T>(this IEnumerable<T> enumerable, T item, IEqualityComparer<T>? comparer = null)
    {
        comparer ??= EqualityComparer<T>.Default;
        return enumerable.Where(m => !comparer.Equals(m, item));
    }

    public static SortedSet<T> ToSortedSet<T>(this IEnumerable<T> enumerable, IComparer<T>? comparer = null)
    {
        return new SortedSet<T>(enumerable, comparer ?? Comparer<T>.Default);
    }

    public static IEnumerable<KeyValuePair<T1, T2>> AsKeyValue<T1, T2>(this IEnumerable<ValueTuple<T1, T2>> enumerable)
    {
        return enumerable.Select(m => m.ToKeyValuePair());
    }

    public static IEnumerable<ValueTuple<T1, T2>> AsTuple<T1, T2>(this IEnumerable<KeyValuePair<T1, T2>> enumerable)
    {
        return enumerable.Select(m => m.ToValueTuple());
    }

    public static TimeSpan Average<T>(this IEnumerable<T> source, Func<T, TimeSpan> selector)
    {
        var ticks = (long)source.Select(m => selector(m).Ticks).Average();
        return TimeSpan.FromTicks(ticks);
    }

    public static TimeSpan Sum<T>(this IEnumerable<T> source, Func<T, TimeSpan> selector)
    {
        var ticks = source.Select(m => selector(m).Ticks).Sum();
        return TimeSpan.FromTicks(ticks);
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

    public static IEnumerable<(T, T)> CrossJoin<T>(this IEnumerable<T> enumerable)
    {
        var items = enumerable.AsIReadOnlyCollection();
        foreach (var a in items)
        {
            foreach (var b in items)
            {
                yield return (a, b);
            }
        }
    }

    public static IEnumerable<(T, T, T)> CrossJoinCube<T>(this IEnumerable<T> enumerable)
    {
        var items = enumerable.AsIReadOnlyCollection();
        foreach (var a in items)
        {
            foreach (var b in items)
            {
                foreach (var c in items)
                {
                    yield return (a, b, c);
                }
            }
        }
    }

    public static IEnumerable<(T1, T2)> CrossJoin<T1, T2>(this IEnumerable<T1> left, IEnumerable<T2> right)
    {
        var items = right.AsIReadOnlyCollection();
        return left.SelectMany(m => items, static (t1, t2) => (t1, t2));
    }

    public static IEnumerable<(T1, T2)> CrossJoin<T1, T2>(this IEnumerable<T1> left, Func<T1, IEnumerable<T2>> right)
    {
        return left.SelectMany(right, static (t1, t2) => (t1, t2));
    }

    public static IEnumerable<T3> Select<T1, T2, T3>(this IEnumerable<(T1, T2)> source, Func<T1, T2, int, T3> selector)
    {
        return source.Select((m, i) => selector(m.Item1, m.Item2, i));
    }

#if !NET9_0_OR_GREATER
    public static IEnumerable<(int Index, T Item)> Index<T>(this IEnumerable<T> enumerable)
    {
        Check.NotNull(enumerable);
        return IndexIterator(enumerable);

        static IEnumerable<(int Index, T Item)> IndexIterator(IEnumerable<T> enumerable)
        {
            var i = 0;
            foreach (var item in enumerable)
            {
                yield return (i++, item);
            }
        }
    }
#endif

#if !NET5_0_OR_GREATER
    public static bool TryGetNonEnumeratedCount<T>([NoEnumeration] this IEnumerable<T> source, out int count)
    {
        switch (source)
        {
            case ICollection<T> genericCollection:
                count = genericCollection.Count;
                return true;
            case IReadOnlyCollection<T> readOnlyCollection:
                count = readOnlyCollection.Count;
                return true;
            case ICollection collection:
                count = collection.Count;
                return true;
            default:
                count = default;
                return false;
        }
    }
#endif

    public static IEnumerable<IndexedItem<T>> IndexEx<T>(this IEnumerable<T> enumerable)
    {
        Check.NotNull(enumerable);

        // we separate the null check from the method body with yield, otherwise the null check will not be executed until start enumerating.
        // see details in https://stackoverflow.com/questions/42149895/method-having-yield-return-is-not-throwing-exception
        return IndexExIterator(enumerable);

        static IEnumerable<IndexedItem<T>> IndexExIterator(IEnumerable<T> enumerable)
        {
            using var enumerator = enumerable.GetEnumerator();

            if (!enumerator.MoveNext())
            {
                yield break;
            }

            var i = 0;
            var current = enumerator.Current;
            while (enumerator.MoveNext())
            {
                yield return new(i, current, i == 0, false);
                current = enumerator.Current;
                ++i;
            }

            yield return new(i, current, i == 0, true);
        }
    }

    public static bool AnyEx<T>(this IEnumerable<T> enumerable)
    {
        Check.NotNull(enumerable);

        return enumerable is IReadOnlyCollection<T> collection
            ? collection.Count > 0
            : enumerable.Any();
    }

    public static IEnumerable<T> Concat<T>(this IEnumerable<IEnumerable<T>> enumerable)
    {
        return enumerable.SelectMany(m => m);
    }

    public static IEnumerable<T> Concat<T>(this IEnumerable<T> source, IEnumerable<T>[] arrays)
    {
        return arrays.Prepend(source).Concat();
    }
    
    public static IOrderedEnumerable<T> OrderBy<T, TKey>(this IEnumerable<T> enumerable, Func<T, TKey> keySelector, bool desc)
    {
        return desc
            ? enumerable.OrderByDescending(keySelector)
            : enumerable.OrderBy(keySelector);
    }

    public static async IAsyncEnumerable<TResult> SelectAsync<T, TResult>(this IEnumerable<T> source, Func<T, Task<TResult>> selector)
    {
        foreach (var item in source)
        {
            yield return await selector(item);
        }
    }

    public static IEnumerable<T> Do<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source)
        {
            action(item);
            yield return item;
        }
    }

    public static async IAsyncEnumerable<T> DoAsync<T>(this IEnumerable<T> source, Func<T, Task> action)
    {
        foreach (var item in source)
        {
            await action(item);
            yield return item;
        }
    }

    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source)
        {
            action(item);
        }
    }

    public static void ForEach<T>(this IEnumerable<T> source, Action<int, T> action)
    {
        var i = 0;
        foreach (var item in source)
        {
            action(i++, item);
        }
    }

    public static async Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> action)
    {
        foreach (var item in source)
        {
            await action(item);
        }
    }

    public static async Task ForEachAsync<T>(this IEnumerable<T> source, Func<int, T, Task> action)
    {
        var i = 0;
        foreach (var item in source)
        {
            await action(i++, item);
        }
    }

    public static IEnumerable<KeyValuePair<TKey, TValue>> EnumerateMany<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, IReadOnlyCollection<TValue>>> enumerable)
    {
        foreach (var (key, values) in enumerable)
        {
            foreach (var value in values)
            {
                yield return new(key, value);
            }
        }
    }

    /// <summary>
    /// Distributes the source elements among at most <paramref name="partitionCount"/> partitions in round-robin order.
    /// </summary>
    /// <remarks>
    /// The source is enumerated when the result is enumerated. Each enumeration starts a new distribution and is
    /// independent of every other enumeration. Empty partitions are not returned.
    /// </remarks>
    public static IEnumerable<IEnumerable<T>> DistributeRoundRobin<T>(this IEnumerable<T> source, int partitionCount)
    {
        Check.NotNull(source);
        Check.Positive(partitionCount);
        return DistributeRoundRobinIterator(source, partitionCount);

        static IEnumerable<IEnumerable<T>> DistributeRoundRobinIterator(IEnumerable<T> source, int partitionCount)
        {
            var index = 0;
            foreach (var partition in source.GroupBy(_ => index++ % partitionCount))
            {
                yield return partition;
            }
        }
    }

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

    /// <summary>
    /// Enumerates the sequence while providing each element together with its immediate predecessor.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="enumerable">The source sequence.</param>
    /// <returns>
    /// A sequence of tuples where:
    /// <list type="bullet">
    /// <item><description><c>Item</c> is the current element.</description></item>
    /// <item><description><c>Previous</c> is the previous element, or <see langword="default"/> for the first element.</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// The source sequence is enumerated only once and evaluated lazily.
    /// </remarks>
    public static IEnumerable<(T Item, T? Previous)> WithPrevious<T>(this IEnumerable<T> enumerable)
    {
        Check.NotNull(enumerable);
        return WithPreviousIterator(enumerable);

        static IEnumerable<(T Item, T? Previous)> WithPreviousIterator(IEnumerable<T> enumerable)
        {
            var previous = default(T);
            foreach (var item in enumerable)
            {
                yield return (item, previous);
                previous = item;
            }
        }
    }

    /// <summary>
    /// Enumerates the sequence while providing each element together with its immediate successor.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="enumerable">The source sequence.</param>
    /// <returns>
    /// A sequence of tuples where:
    /// <list type="bullet">
    /// <item><description><c>Item</c> is the current element.</description></item>
    /// <item><description><c>Next</c> is the next element, or <see langword="default"/> for the last element.</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// The source sequence is enumerated only once and evaluated lazily.
    /// </remarks>
    public static IEnumerable<(T Item, T? Next)> WithNext<T>(this IEnumerable<T> enumerable)
    {
        Check.NotNull(enumerable);
        return WithNextIterator(enumerable);

        static IEnumerable<(T Item, T? Next)> WithNextIterator(IEnumerable<T> enumerable)
        {
            using var enumerator = enumerable.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                yield break;
            }

            var current = enumerator.Current;
            while (enumerator.MoveNext())
            {
                var next = enumerator.Current;
                yield return (current, next);
                current = next;
            }

            yield return (current, default);
        }
    }

    /// <summary>
    /// Enumerates the sequence while providing each element together with its
    /// immediate predecessor and successor.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="enumerable">The source sequence.</param>
    /// <returns>
    /// A sequence of tuples where:
    /// <list type="bullet">
    /// <item><description><c>Item</c> is the current element.</description></item>
    /// <item><description><c>Previous</c> is the previous element, or <see langword="default"/> for the first element.</description></item>
    /// <item><description><c>Next</c> is the next element, or <see langword="default"/> for the last element.</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// The source sequence is enumerated only once and evaluated lazily.
    /// </remarks>
    public static IEnumerable<(T Item, T? Previous, T? Next)> WithNeighbors<T>(this IEnumerable<T> enumerable)
    {
        Check.NotNull(enumerable);
        return WithNeighborsIterator(enumerable);

        static IEnumerable<(T Item, T? Previous, T? Next)> WithNeighborsIterator(IEnumerable<T> enumerable)
        {
            using var enumerator = enumerable.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                yield break;
            }

            var previous = default(T);
            var current = enumerator.Current;
            while (enumerator.MoveNext())
            {
                var next = enumerator.Current;
                yield return (current, previous, next);
                previous = current;
                current = next;
            }

            yield return (current, previous, default);
        }
    }

#if NET7_0_OR_GREATER
    public static T Sum<T>(this IEnumerable<T> enumerable) where T : IAdditionOperators<T, T, T>, IAdditiveIdentity<T, T>
    {
        return enumerable.Aggregate(T.AdditiveIdentity, (current, item) => current + item);
    }
#endif

#if !NET5_0_OR_GREATER
    /// <summary>Produces a sequence of tuples with elements from the two specified sequences.</summary>
    /// <param name="first">The first sequence to merge.</param>
    /// <param name="second">The second sequence to merge.</param>
    /// <typeparam name="TFirst">The type of the elements of the first input sequence.</typeparam>
    /// <typeparam name="TSecond">The type of the elements of the second input sequence.</typeparam>
    /// <returns>A sequence of tuples with elements taken from the first and second sequences, in that order.</returns>
    public static IEnumerable<(TFirst First, TSecond Second)> Zip<TFirst, TSecond>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second)
    {
        return first.Zip(second, (f, s) => (f, s));
    }
#endif

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

    public static IEnumerable<T> SelectIf<T>(this IEnumerable<T> enumerable, Func<T, T> selector, bool condition)
    {
        return condition
            ? enumerable.Select(selector)
            : enumerable;
    }

    public static IEnumerable<T> SelectIf<T>(this IEnumerable<T> enumerable, bool condition, Func<T, T> selector)
        => enumerable.SelectIf(selector, condition);

    public static IEnumerable<T> SelectIf<T>(this IEnumerable<T> enumerable, Func<T, int, T> selector, bool condition)
    {
        return condition
            ? enumerable.Select(selector)
            : enumerable;
    }

    public static IEnumerable<T> SelectIf<T>(this IEnumerable<T> enumerable, bool condition, Func<T, int, T> selector)
        => enumerable.SelectIf(selector, condition);

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

    public static T Sample<T>(this IEnumerable<T> source, Random? random = null)
    {
        return (random ?? Random.Shared).Sample(source);
    }
    
    extension<T>(IEnumerable<T>)
    {
        public static IEnumerable<T> operator +(IEnumerable<T> enumerable, IEnumerable<T> other)
        {
            return enumerable.Concat(other);
        }

        public static IEnumerable<T> operator +(IEnumerable<T> enumerable, T item)
        {
            return enumerable.Append(item);
        }

        public static IEnumerable<T> operator +(T item, IEnumerable<T> enumerable)
        {
            return enumerable.Prepend(item);
        }
    }
}
