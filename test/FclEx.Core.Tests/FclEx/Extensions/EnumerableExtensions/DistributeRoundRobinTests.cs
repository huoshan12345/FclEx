namespace FclEx.Extensions.EnumerableExtensions;

public class DistributeRoundRobinTests
{
    [Fact]
    public void DistributeRoundRobin_DistributesElementsByIndex()
    {
        var actual = Enumerable.Range(0, 8)
            .DistributeRoundRobin(3)
            .Select(partition => partition.ToArray())
            .ToArray();

        Assert.Equal([0, 3, 6], actual[0]);
        Assert.Equal([1, 4, 7], actual[1]);
        Assert.Equal([2, 5], actual[2]);
    }

    [Fact]
    public void DistributeRoundRobin_ReenumerationStartsFromFirstPartition()
    {
        var distribution = Enumerable.Range(0, 6).DistributeRoundRobin(3);

        var first = distribution.Select(partition => partition.ToArray()).ToArray();
        var second = distribution.Select(partition => partition.ToArray()).ToArray();

        Assert.Equal(first, second);
    }

    [Fact]
    public void DistributeRoundRobin_DoesNotReturnEmptyPartitions()
    {
        var actual = new[] { 1, 2 }.DistributeRoundRobin(5).ToArray();

        Assert.Equal(2, actual.Length);
    }

    [Fact]
    public void DistributeRoundRobin_ValidatesArgumentsImmediately()
    {
        IEnumerable<int> source = null!;

        Assert.Throws<ArgumentNullException>(() => source.DistributeRoundRobin(1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Array.Empty<int>().DistributeRoundRobin(0));
    }
}
