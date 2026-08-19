namespace FclEx.Extensions;

public static partial class EnumerableExtensions
{
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
}
