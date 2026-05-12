namespace FclEx.Extensions;

public static class ReadOnlyListExtensions
{
    public static bool TryGet<T>(this IReadOnlyList<T> list, int index, [NotNullWhen(true)] out T? value)
    {
        if (index >= 0 && index < list.Count)
        {
            value = list[index]!;
            return true;
        }

        value = default;
        return false;
    }

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static T? Get<T>(this IReadOnlyList<T> list, int index, T? defaultValue = default)
    {
        return list.TryGet(index, out var value)
            ? value
            : defaultValue;
    }

    public static T Sample<T>(this IReadOnlyList<T> list, Random? random = null)
    {
        return (random ?? Random.Shared).Sample(list);
    }
}