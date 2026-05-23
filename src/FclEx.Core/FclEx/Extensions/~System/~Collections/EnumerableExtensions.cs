namespace FclEx.Extensions;

public static partial class EnumerableExtensions
{
    public static bool IsNullOrEmpty(this IEnumerable? enumerable)
    {
        return enumerable is null || enumerable.Any() == false;
    }

    public static bool Any(this IEnumerable enumerable)
    {
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var _ in enumerable)
        {
            return true;
        }

        return false;
    }
}