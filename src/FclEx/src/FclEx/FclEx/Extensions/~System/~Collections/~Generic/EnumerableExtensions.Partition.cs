namespace FclEx.Extensions;

partial class EnumerableExtensions
{
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

        return (trueList.AsSpan().ToArray(), falseList.AsSpan().ToArray());
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