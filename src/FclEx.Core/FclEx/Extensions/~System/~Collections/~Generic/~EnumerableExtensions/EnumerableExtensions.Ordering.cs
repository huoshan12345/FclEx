namespace FclEx.Extensions;

public static partial class EnumerableExtensions
{
    public static IOrderedEnumerable<T> OrderBy<T, TKey>(this IEnumerable<T> enumerable, Func<T, TKey> keySelector, bool desc)
    {
        return desc
            ? enumerable.OrderByDescending(keySelector)
            : enumerable.OrderBy(keySelector);
    }
}
