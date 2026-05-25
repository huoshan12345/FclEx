namespace FclEx.YamlDotNet;

public class YamlSequenceNodeExtensionsTests
{
    [Fact]
    public void TryRemoveChildren_RemovesAllMatchedChildren()
    {
        var first = new YamlScalarNode("remove");
        var second = new YamlScalarNode("keep");
        var third = new YamlScalarNode("remove");
        var node = new YamlSequenceNode(first, second, third);

        var removed = node.TryRemoveChildren<YamlScalarNode>(m => m.Value == "remove", out var removedNodes);

        Assert.True(removed);
        Assert.Equal(new[] { first, third }, removedNodes);
        var remaining = Assert.Single(node.Children);
        Assert.Same(second, remaining);
    }

    [Fact]
    public void TryRemoveChildren_WhenNothingMatches_ShouldReturnFalse()
    {
        var child = new YamlScalarNode("keep");
        var node = new YamlSequenceNode(child);

        var removed = node.TryRemoveChildren<YamlScalarNode>(m => m.Value == "remove", out var removedNodes);

        Assert.False(removed);
        Assert.Empty(removedNodes);
        var remaining = Assert.Single(node.Children);
        Assert.Same(child, remaining);
    }
}
