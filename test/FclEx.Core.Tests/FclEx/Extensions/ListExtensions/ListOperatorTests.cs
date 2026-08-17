namespace FclEx.Extensions;

public class ListOperatorTests
{
    [Fact]
    public void PlusItem_ShouldCreateANewList()
    {
        var original = new List<int> { 1, 2 };

        var result = original + 3;

        Assert.NotSame(original, result);
        Assert.Equal([1, 2], original);
        Assert.Equal([1, 2, 3], result);
    }

    [Fact]
    public void PlusEqualsItem_ShouldMutateTheExistingList()
    {
        var list = new List<int> { 1, 2 };
        var original = list;

        list += 3;

        Assert.Same(original, list);
        Assert.Equal([1, 2, 3], list);
    }

    [Fact]
    public void PlusEqualsEnumerable_ShouldMutateTheExistingList()
    {
        var list = new List<int> { 1 };
        var original = list;
        IEnumerable<int> additional = [2, 3];

        list += additional;

        Assert.Same(original, list);
        Assert.Equal([1, 2, 3], list);
    }
}
