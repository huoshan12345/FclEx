namespace FclEx.Extensions;

public static partial class EnumerableExtensions
{
    /// <summary>
    /// Distributes the source elements among at most <paramref name="partitionCount"/> partitions in round-robin order.
    /// </summary>
    /// <remarks>
    /// The source is enumerated when the result is enumerated. Each enumeration starts a new distribution and is
    /// independent of every other enumeration. Empty partitions are not returned.
    /// </remarks>
    public static IEnumerable<IEnumerable<T>> DistributeRoundRobin<T>(this IEnumerable<T> source, int partitionCount)
    {
        Check.NotNull(source);
        Check.Positive(partitionCount);
        return DistributeRoundRobinIterator(source, partitionCount);

        static IEnumerable<IEnumerable<T>> DistributeRoundRobinIterator(IEnumerable<T> source, int partitionCount)
        {
            var index = 0;
            foreach (var partition in source.GroupBy(_ => index++ % partitionCount))
            {
                yield return partition;
            }
        }
    }
}
