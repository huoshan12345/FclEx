namespace FclEx.Extensions;

public static partial class EnumerableExtensions
{
    public static IEnumerable<(T, T)> CrossJoin<T>(this IEnumerable<T> enumerable)
    {
        var items = enumerable.AsIReadOnlyCollection();
        foreach (var a in items)
        {
            foreach (var b in items)
            {
                yield return (a, b);
            }
        }
    }

    public static IEnumerable<(T, T, T)> CrossJoinCube<T>(this IEnumerable<T> enumerable)
    {
        var items = enumerable.AsIReadOnlyCollection();
        foreach (var a in items)
        {
            foreach (var b in items)
            {
                foreach (var c in items)
                {
                    yield return (a, b, c);
                }
            }
        }
    }

    /// <summary>Produces the Cartesian product of <paramref name="left"/> and <paramref name="right"/>.</summary>
    /// <remarks>
    /// <paramref name="right"/> is materialized once before the product is returned, so it may be a one-shot sequence.
    /// The left sequence remains deferred and is enumerated when the result is enumerated.
    /// </remarks>
    public static IEnumerable<(T1, T2)> CrossJoin<T1, T2>(this IEnumerable<T1> left, IEnumerable<T2> right)
    {
        var items = right.AsIReadOnlyCollection();
        return left.SelectMany(m => items, static (t1, t2) => (t1, t2));
    }

    public static IEnumerable<(T1, T2)> CrossJoin<T1, T2>(this IEnumerable<T1> left, Func<T1, IEnumerable<T2>> right)
    {
        return left.SelectMany(right, static (t1, t2) => (t1, t2));
    }
}
