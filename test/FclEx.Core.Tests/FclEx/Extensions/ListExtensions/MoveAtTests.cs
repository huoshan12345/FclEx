namespace FclEx.Extensions.ListExtensions;

public class MoveAtTests
{
    [Fact]
    public void MoveAt_WhenMovingForward_ShouldMoveItemToTargetIndex()
    {
        var list = new List<string> { "A", "B", "C", "D", "E" };
        list.MoveAt(1, 3);
        Assert.Equal(new[] { "A", "C", "D", "B", "E" }, list);
    }

    [Fact]
    public void MoveAt_WhenMovingBackward_ShouldMoveItemToTargetIndex()
    {
        var list = new List<string> { "A", "B", "C", "D", "E" };
        list.MoveAt(3, 1);
        Assert.Equal(new[] { "A", "D", "B", "C", "E" }, list);
    }

    [Fact]
    public void MoveAt_WhenOldIndexEqualsNewIndex_ShouldNotChangeList()
    {
        var list = new List<string> { "A", "B", "C" };
        list.MoveAt(1, 1);
        Assert.Equal(new[] { "A", "B", "C" }, list);
    }

    [Fact]
    public void MoveAt_WhenMovingFirstItemToLastIndex_ShouldMoveItemToEnd()
    {
        var list = new List<string> { "A", "B", "C", "D" };
        list.MoveAt(0, 3);
        Assert.Equal(new[] { "B", "C", "D", "A" }, list);
    }

    [Fact]
    public void MoveAt_WhenMovingLastItemToFirstIndex_ShouldMoveItemToStart()
    {
        var list = new List<string> { "A", "B", "C", "D" };
        list.MoveAt(3, 0);
        Assert.Equal(new[] { "D", "A", "B", "C" }, list);
    }

    [Fact]
    public void MoveAt_WhenListIsNull_ShouldThrowArgumentNullException()
    {
        List<string>? list = null;
        var exception = Assert.Throws<ArgumentNullException>(() => list!.MoveAt(0, 1));
        Assert.Equal("list", exception.ParamName);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(3, 0)]
    public void MoveAt_WhenOldIndexIsOutOfRange_ShouldThrowArgumentOutOfRangeException(int oldIndex, int newIndex)
    {
        var list = new List<string> { "A", "B", "C" };
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => list.MoveAt(oldIndex, newIndex));
        Assert.Equal("oldIndex", exception.ParamName);
    }

    [Theory]
    [InlineData(0, -1)]
    [InlineData(0, 3)]
    public void MoveAt_WhenNewIndexIsOutOfRange_ShouldThrowArgumentOutOfRangeException(int oldIndex, int newIndex)
    {
        var list = new List<string> { "A", "B", "C" };
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => list.MoveAt(oldIndex, newIndex));
        Assert.Equal("newIndex", exception.ParamName);
    }
}
