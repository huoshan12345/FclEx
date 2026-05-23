namespace FclEx.Extensions;

public static class EqualityComparerExtensions
{
    public static int GetHashCodeOrDefault<T>(this IEqualityComparer<T> comparer, T? obj)
    {
        return obj is null
            ? 0
            : comparer.GetHashCode(obj);
    }
}
