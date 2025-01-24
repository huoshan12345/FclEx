namespace FclEx.Extensions;

public static class GroupingExtensions
{
    public static void Deconstruct<TKey, TElement>(this IGrouping<TKey, TElement> group, out TKey key, out IEnumerable<TElement> enumerable)
    {
        key = group.Key;
        enumerable = group;
    }
}