namespace System.Collections.Generic.OrderedList;

public class OrderedListBoundsTests
{
    [Fact]
    public void Bounds_ShouldValidateTheCompleteSearchRange()
    {
        var list = new OrderedList<int>([1, 2, 3]);

        foreach (var bound in new Func<int>[]
                 {
                     () => list.LowerBound(2, -1, 2),
                     () => list.UpperBound(2, -1, 2),
                     () => list.LowerBound(2, 4, 4),
                     () => list.UpperBound(2, 4, 4),
                     () => list.LowerBound(2, 0, -1),
                     () => list.UpperBound(2, 0, -1),
                     () => list.LowerBound(2, 0, 4),
                     () => list.UpperBound(2, 0, 4),
                 })
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => bound());
        }

        Assert.Throws<ArgumentException>(() => list.LowerBound(2, 2, 1));
        Assert.Throws<ArgumentException>(() => list.UpperBound(2, 2, 1));
        Assert.Equal(3, list.LowerBound(2, 3, 3));
        Assert.Equal(3, list.UpperBound(2, 3, 3));

        Assert.Equal("lower", Assert.Throws<ArgumentOutOfRangeException>(() => list.LowerBound(2, -1, 2)).ParamName);
        Assert.Equal("upper", Assert.Throws<ArgumentOutOfRangeException>(() => list.UpperBound(2, 0, 4)).ParamName);
        Assert.Equal("upper", Assert.Throws<ArgumentException>(() => list.LowerBound(2, 2, 1)).ParamName);
    }

    [Fact]
    public void EqualRange_ShouldReturnEveryMatchAndHandleMissingItems()
    {
        var list = new OrderedList<int>([1, 2, 2, 2, 3]);

        Assert.Equal([2, 2, 2], list.EqualRange(2));
        Assert.Empty(list.EqualRange(4));
    }

    [Fact]
    public void RemoveRange_ShouldRemoveAnInclusiveRange()
    {
        var list = new OrderedList<int>([1, 2, 2, 3, 4, 4, 5]);

        var removed = list.RemoveRange(2, 4);

        Assert.Equal(5, removed);
        Assert.Equal([1, 5], list);
    }

    [Fact]
    public void RemoveRange_ShouldRejectAnInvertedRangeWithoutChangingTheList()
    {
        var list = new OrderedList<int>([1, 2, 3]);

        Assert.Throws<ArgumentException>(() => list.RemoveRange(3, 2));

        Assert.Equal([1, 2, 3], list);
    }
}
