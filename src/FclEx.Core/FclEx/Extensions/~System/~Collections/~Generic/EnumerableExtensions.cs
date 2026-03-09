namespace FclEx.Extensions;

public readonly record struct IndexedItem<T>(int Index, T Item, bool IsFirst, bool IsLast);

public static partial class EnumerableExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this IEnumerable<T>? source) => source is null || source.AnyEx() == false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEmpty<T>(this IEnumerable<T> source) => source.AnyEx() == false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNotEmpty<T>([NotNullWhen(true)] this IEnumerable<T>? enumerable) => enumerable.IsNullOrEmpty() == false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? source) => source ?? [];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> NotNull<T>(this IEnumerable<T?> enumerable) where T : struct
        => enumerable.Where(m => m is not null).Select(m => m.Get());

    /// <summary>
    /// Filters the elements of a collection to exclude those that satisfy the given predicate.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="enumerable">The collection of elements to filter.</param>
    /// <param name="predicate">The predicate function that defines the condition to exclude.</param>
    /// <returns>An IEnumerable containing elements that do not satisfy the predicate.</returns>
    public static IEnumerable<T> Not<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
        => enumerable.Where(m => predicate(m) == false);

    public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate, bool condition)
        => condition ? enumerable.Where(predicate) : enumerable;

    public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> enumerable, bool condition, Func<T, bool> predicate)
        => enumerable.WhereIf(predicate, condition);

    public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> enumerable, Func<T, int, bool> predicate, bool condition)
        => condition ? enumerable.Where(predicate) : enumerable;

    public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> enumerable, bool condition, Func<T, int, bool> predicate)
        => enumerable.WhereIf(predicate, condition);

    public static IEnumerable<T> TryTake<T>(this IEnumerable<T> enumerable, int? count)
    {
        return count is { } c ? enumerable.Take(c) : enumerable;
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

    public static int BitsToInt(this IEnumerable<bool> bits)
    {
        var num = 0;
        foreach (var (i, b) in bits.Index())
        {
            var bit = b ? 1 : 0;
            num &= (bit << i);
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
        return left.SelectMany(m => right, static (t1, t2) => (t1, t2));
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

    public static IEnumerable<T> Concat<T>(this IEnumerable<IEnumerable<T>> arrays)
    {
        return arrays.SelectMany(m => m);
    }

    public static IEnumerable<T> Concat<T>(this IEnumerable<T> source, IEnumerable<T>[] arrays)
    {
        return arrays.Prepend(source).Concat();
    }

    public static IEnumerable<(T Left, T2 Right)> SelectMany<T, T2>(this IEnumerable<T> left, IEnumerable<T2> right)
    {
        return left.SelectMany(_ => right, (x, y) => (x, y));
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

    public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> list, int parts)
    {
        var i = 0;
        var splits = from item in list
                     group item by i++ % parts into part
                     select part;
        return splits;
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
    /// Thrown if <paramref name="first"/> or <paramref name="second"/> is <c>null</c>.
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

#if NETSTANDARD2_0
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