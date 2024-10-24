using System.Collections.ObjectModel;

namespace FclEx.Extensions;

[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
public static partial class EnumerableExtensions
{
    public static TCollection AsCollection<T, TCollection>(this IEnumerable<T> source, Func<IEnumerable<T>, TCollection> factory)
    {
        return source switch
        {
            null => throw new ArgumentNullException(nameof(source)),
            TCollection col => col,
            _ => factory(source),
        };
    }

    public static ReadOnlyCollection<T> AsReadOnlyCollection<T>(this IEnumerable<T> source)
    {
        return source.AsCollection(m => new ReadOnlyCollection<T>(m.ToArray()));
    }

    public static IList<T> AsIList<T>(this IEnumerable<T> source)
    {
        return source.AsCollection<T, IList<T>>(m => m.ToList());
    }

    public static ISet<T> AsISet<T>(this IEnumerable<T> source)
    {
        return source.AsCollection<T, ISet<T>>(m => m.ToHashSet());
    }

    public static ICollection<T> AsICollection<T>(this IEnumerable<T> source)
    {
        return source.AsCollection<T, ICollection<T>>(m => m.ToList());
    }

    public static IReadOnlyList<T> AsIReadOnlyList<T>(this IEnumerable<T> source)
    {
        return source.AsCollection<T, IReadOnlyList<T>>(m => m.ToList());
    }

    public static IReadOnlyCollection<T> AsIReadOnlyCollection<T>(this IEnumerable<T> source)
    {
        return source.AsCollection<T, IReadOnlyCollection<T>>(m => m.ToList());
    }

    public static List<T> AsList<T>(this IEnumerable<T> source)
    {
        return source.AsCollection<T, List<T>>(m => m.ToList());
    }

    public static T[] AsArray<T>(this IEnumerable<T> source)
    {
        return source.AsCollection<T, T[]>(m => m.ToArray());
    }
}