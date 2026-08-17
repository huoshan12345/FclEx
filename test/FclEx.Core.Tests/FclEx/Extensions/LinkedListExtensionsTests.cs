namespace FclEx.Extensions;

public class LinkedListExtensionsTests
{
    [Fact]
    public void AdditionOperators_ReturnNewListsWithoutMutatingTheirOperands()
    {
        var list = new LinkedList<int>([1, 2]);

        var appendItem = list + 3;
        var prependItem = 0 + list;
        var appendItems = list + new[] { 3, 4 };
        var prependItems = new[] { -1, 0 } + list;

        Assert.Equal(new[] { 1, 2 }, list);
        Assert.Equal(new[] { 1, 2, 3 }, appendItem);
        Assert.Equal(new[] { 0, 1, 2 }, prependItem);
        Assert.Equal(new[] { 1, 2, 3, 4 }, appendItems);
        Assert.Equal(new[] { -1, 0, 1, 2 }, prependItems);
    }
}
