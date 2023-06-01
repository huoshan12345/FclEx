using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace FclEx.Extensions;

[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
public static partial class EnumerableExtensions
{
    public static ReadOnlyCollection<T> AsReadOnly<T>(this IEnumerable<T> source)
    {
        return source switch
        {
            null => throw new ArgumentNullException(nameof(source)),
            ReadOnlyCollection<T> col => col,
            _ => new ReadOnlyCollection<T>(source.ToList())
        };
    }

    public static IList<T> AsIList<T>(this IEnumerable<T> source)
    {
        return source switch
        {
            null => throw new ArgumentNullException(nameof(source)),
            IList<T> col => col,
            _ => source.ToList()
        };
    }

    public static ICollection<T> AsICollection<T>(this IEnumerable<T> source)
    {
        return source switch
        {
            null => throw new ArgumentNullException(nameof(source)),
            ICollection<T> col => col,
            _ => source.ToList()
        };
    }

    public static IReadOnlyList<T> AsIReadOnlyList<T>(this IEnumerable<T> source)
    {
        return source switch
        {
            null => throw new ArgumentNullException(nameof(source)),
            IReadOnlyList<T> col => col,
            _ => source.ToList()
        };
    }

    public static List<T> AsList<T>(this IEnumerable<T> source)
    {
        return source switch
        {
            null => throw new ArgumentNullException(nameof(source)),
            List<T> col => col,
            _ => source.ToList()
        };
    }

    public static T[] AsArray<T>(this IEnumerable<T> source)
    {
        return source switch
        {
            null => throw new ArgumentNullException(nameof(source)),
            T[] col => col,
            _ => source.ToArray()
        };
    }
}