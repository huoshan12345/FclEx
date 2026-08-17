namespace FclEx.Extensions;

public class QueueExtensionsTests
{
    [Fact]
    public void Dequeue_ImmediatelyRemovesAndMaterializesRequestedItems()
    {
        var queue = new Queue<int>([1, 2, 3]);

        var items = queue.Dequeue(2);

        Assert.Equal(new[] { 1, 2 }, items);
        Assert.Equal(new[] { 3 }, queue);
    }

    [Fact]
    public void Dequeue_WithNegativeCount_ThrowsArgumentOutOfRangeException()
    {
        var queue = new Queue<int>();

        Assert.Throws<ArgumentOutOfRangeException>(() => queue.Dequeue(-1));
    }
}
