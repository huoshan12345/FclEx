namespace FclEx.YamlDotNet;

public class OrderedDictionaryExtensionsTests
{
    [Fact]
    public void Swap_ExchangesItemsAtIndexes()
    {
        var node = CreateNode();

        node.Children.Swap(0, 2);

        Assert.Equal(new[] { "third", "second", "first" }, GetKeys(node));
    }

    [Fact]
    public void Swap_WithSameIndexDoesNothing()
    {
        var node = CreateNode();

        node.Children.Swap(1, 1);

        Assert.Equal(new[] { "first", "second", "third" }, GetKeys(node));
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, 3)]
    public void Swap_ThrowsWhenIndexIsOutOfRange(int index1, int index2)
    {
        var node = CreateNode();

        Assert.Throws<ArgumentOutOfRangeException>(() => node.Children.Swap(index1, index2));
    }

    [Fact]
    public void MoveAt_MovesItemForward()
    {
        var node = CreateNode();

        node.Children.MoveAt(0, 2);

        Assert.Equal(new[] { "second", "third", "first" }, GetKeys(node));
    }

    [Fact]
    public void MoveAt_MovesItemBackward()
    {
        var node = CreateNode();

        node.Children.MoveAt(2, 0);

        Assert.Equal(new[] { "third", "first", "second" }, GetKeys(node));
    }

    [Fact]
    public void MoveAt_WithSameIndexDoesNothing()
    {
        var node = CreateNode();

        node.Children.MoveAt(1, 1);

        Assert.Equal(new[] { "first", "second", "third" }, GetKeys(node));
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(3, 0)]
    [InlineData(0, 3)]
    public void MoveAt_ThrowsWhenIndexIsOutOfRange(int sourceIndex, int destinationIndex)
    {
        var node = CreateNode();

        Assert.Throws<ArgumentOutOfRangeException>(() => node.Children.MoveAt(sourceIndex, destinationIndex));
    }

    private static YamlMappingNode CreateNode()
    {
        return new YamlMappingNode
        {
            { "first", "1" },
            { "second", "2" },
            { "third", "3" },
        };
    }

    private static string?[] GetKeys(YamlMappingNode node)
    {
        return node.GetChildren().Select(m => m.Key.Value).ToArray();
    }
}
