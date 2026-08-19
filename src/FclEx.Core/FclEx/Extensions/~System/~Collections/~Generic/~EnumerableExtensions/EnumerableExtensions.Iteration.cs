namespace FclEx.Extensions;

public static partial class EnumerableExtensions
{
    public static IEnumerable<T> Do<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source)
        {
            action(item);
            yield return item;
        }
    }

    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source)
        {
            action(item);
        }
    }

    public static void ForEach<T>(this IEnumerable<T> source, Action<int, T> action)
    {
        var i = 0;
        foreach (var item in source)
        {
            action(i++, item);
        }
    }
}
