namespace FclEx.Extensions;

public static partial class EnumerableExtensions
{
    public static T Sample<T>(this IEnumerable<T> source, Random? random = null)
    {
        return (random ?? Random.Shared).Sample(source);
    }
}
