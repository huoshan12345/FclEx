namespace System.Collections.Generic;

public class DequeTests
{
    [Fact]
    public void EmptyDequeOperations_ShouldUseTheStandardThrowingAndTryContracts()
    {
        var deque = new Deque<int>();

        Assert.Throws<InvalidOperationException>(() => deque.PeekHead());
        Assert.Throws<InvalidOperationException>(() => deque.PeekTail());
        Assert.Throws<InvalidOperationException>(() => deque.DequeueHead());
        Assert.Throws<InvalidOperationException>(() => deque.DequeueTail());

        Assert.False(deque.TryPeekHead(out _));
        Assert.False(deque.TryPeekTail(out _));
        Assert.False(deque.TryDequeueHead(out _));
        Assert.False(deque.TryDequeueTail(out _));
        Assert.Equal(0, deque.Count);
    }

    [Fact]
    public void FailedEmptyOperation_ShouldNotCorruptAnAllocatedDeque()
    {
        var deque = new Deque<int>();
        deque.EnqueueTail(1);
        Assert.Equal(1, deque.DequeueHead());

        Assert.Throws<InvalidOperationException>(() => deque.DequeueHead());

        deque.EnqueueHead(2);
        deque.EnqueueTail(3);
        Assert.True(deque.TryDequeueHead(out var head));
        Assert.True(deque.TryDequeueTail(out var tail));
        Assert.Equal(2, head);
        Assert.Equal(3, tail);
        Assert.Empty(deque);
    }
}
