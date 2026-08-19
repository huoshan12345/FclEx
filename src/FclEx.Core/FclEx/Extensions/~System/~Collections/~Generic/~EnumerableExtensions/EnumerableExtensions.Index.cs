namespace FclEx.Extensions;

public readonly record struct IndexedItem<T>(int Index, T Item, bool IsFirst, bool IsLast);

public static partial class EnumerableExtensions
{
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
}
