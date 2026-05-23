namespace FclEx.Extensions.EnumerableExtensions;

public class PartitionTests
{
    [Fact]
    public void Test()
    {
        var all = Enumerable.Range(1, 100).ToArray();
        var (even, odd) = all.Partition(m => m % 2 == 0);

        Assert.Equal(all.Where(m => m % 2 == 0), even);
        Assert.Equal(all.Where(m => m % 2 != 0), odd);
    }
}