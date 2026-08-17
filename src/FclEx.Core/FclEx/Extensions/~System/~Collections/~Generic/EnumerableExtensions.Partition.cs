namespace FclEx.Extensions;

partial class EnumerableExtensions
{
    /// <summary>
    /// Partitions a sequence into two lists based on a predicate function.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="enumerable">The input sequence to partition.</param>
    /// <param name="predicate">A function that determines the criteria for partitioning the elements. 
    /// Elements that return <see langword="true"/> are placed in the "True" list, while those that return <see langword="false"/> 
    /// are placed in the "False" list.</param>
    /// <returns>A tuple containing two <see cref="IReadOnlyList{T}"/>: 
    /// the first list contains elements for which the predicate returned <see langword="true"/>, 
    /// and the second list contains elements for which the predicate returned <see langword="false"/>.</returns>
    /// <remarks>
    /// This method initializes two lists, iterates through the input sequence, 
    /// and assigns each element to the appropriate list based on the evaluation of the predicate.
    /// If the predicate returns <see langword="true"/> for an item, it is added to the "True" list; 
    /// otherwise, it goes into the "False" list. The lists are returned as arrays wrapped in an IReadOnlyList interface.
    /// </remarks>
    public static (IReadOnlyList<T> True, IReadOnlyList<T> False) Partition<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
    {
        List<T>? trueList = null;
        List<T>? falseList = null;

        foreach (var item in enumerable)
        {
            // NOTE: use ref to initialize trueList or falseList.
            ref var list = ref predicate(item)
                ? ref trueList
                : ref falseList;

            list ??= [];
            list.Add(item);
        }

        return (trueList.AsReadOnlySpan().ToArray(), falseList.AsReadOnlySpan().ToArray());
    }

    public static (IEnumerable<TMember> True, IEnumerable<TMember> False) Partition<T, TMember>(this IEnumerable<T> enumerable,
        Func<T, bool> predicate, Func<T, TMember> selector)
    {
        var (@true, @false) = enumerable.Partition(predicate);
        return (@true.Select(selector), @false.Select(selector));
    }

    public static (TResult True, TResult False) Partition<T, TResult>(this IEnumerable<T> enumerable,
        Func<T, bool> predicate, Func<IEnumerable<T>, TResult> selector)
    {
        var (@true, @false) = enumerable.Partition(predicate);
        return (selector(@true), selector(@false));
    }

    public static (T[] True, T[] False) PartitionToArray<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        var (@true, @false) = source.Partition(predicate);
        return (@true.ToArray(), @false.ToArray());
    }

    public static (List<T> True, List<T> False) PartitionToList<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        var (@true, @false) = source.Partition(predicate);
        return (@true.ToList(), @false.ToList());
    }
}