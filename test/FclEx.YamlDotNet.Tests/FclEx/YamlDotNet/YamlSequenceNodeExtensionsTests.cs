namespace FclEx.YamlDotNet;

public class YamlSequenceNodeExtensionsTests
{
    [Fact]
    public void FindChild_ReturnsFirstMatchedChildAndIndex()
    {
        var first = new YamlScalarNode("skip");
        var second = new YamlScalarNode("match");
        var third = new YamlScalarNode("match");
        var node = new YamlSequenceNode(first, second, third);

        var (child, index) = node.FindChild<YamlScalarNode>(m => m.Value == "match");

        Assert.Same(second, child);
        Assert.Equal(1, index);
    }

    [Fact]
    public void FindChild_IgnoresChildrenWithDifferentType()
    {
        var scalar = new YamlScalarNode("match");
        var mapping = new YamlMappingNode();
        var node = new YamlSequenceNode(scalar, mapping);

        var (child, index) = node.FindChild<YamlMappingNode>(_ => true);

        Assert.Same(mapping, child);
        Assert.Equal(1, index);
    }

    [Fact]
    public void FindChild_ReturnsNullAndMinusOneWhenNoChildMatches()
    {
        var node = new YamlSequenceNode(new YamlScalarNode("skip"));

        var (child, index) = node.FindChild<YamlScalarNode>(m => m.Value == "match");

        Assert.Null(child);
        Assert.Equal(-1, index);
    }

    [Fact]
    public void FindChildren_ReturnsAllMatchedChildrenWithOriginalIndexes()
    {
        var first = new YamlScalarNode("match");
        var second = new YamlMappingNode();
        var third = new YamlScalarNode("match");
        var node = new YamlSequenceNode(first, second, third);

        var matches = node.FindChildren<YamlScalarNode>(m => m.Value == "match");

        Assert.Equal(2, matches.Count);
        Assert.Same(first, matches[0].Child);
        Assert.Equal(0, matches[0].Index);
        Assert.Same(third, matches[1].Child);
        Assert.Equal(2, matches[1].Index);
    }

    [Fact]
    public void FindChildren_ReturnsEmptyListWhenNoChildMatches()
    {
        var node = new YamlSequenceNode(new YamlScalarNode("skip"));

        var matches = node.FindChildren<YamlScalarNode>(m => m.Value == "match");

        Assert.Empty(matches);
    }

    [Fact]
    public void HasChild_ReturnsTrueWhenChildMatches()
    {
        var node = new YamlSequenceNode(new YamlScalarNode("match"));

        var result = node.HasChild<YamlScalarNode>(m => m.Value == "match");

        Assert.True(result);
    }

    [Fact]
    public void HasChild_ReturnsFalseWhenNoChildMatches()
    {
        var node = new YamlSequenceNode(new YamlScalarNode("skip"));

        var result = node.HasChild<YamlScalarNode>(m => m.Value == "match");

        Assert.False(result);
    }

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
    public void TryRemoveChildren_IgnoresChildrenWithDifferentType()
    {
        var scalar = new YamlScalarNode("remove");
        var mapping = new YamlMappingNode();
        var node = new YamlSequenceNode(scalar, mapping);

        var removed = node.TryRemoveChildren<YamlMappingNode>(_ => true, out var removedNodes);

        Assert.True(removed);
        Assert.Equal(new[] { mapping }, removedNodes);
        Assert.Equal(new[] { scalar }, node.Children);
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

    [Fact]
    public void TryRemoveChildren_WithoutOutParameterRemovesMatchedChildren()
    {
        var first = new YamlScalarNode("remove");
        var second = new YamlScalarNode("keep");
        var node = new YamlSequenceNode(first, second);

        var removed = node.TryRemoveChildren<YamlScalarNode>(m => m.Value == "remove");

        Assert.True(removed);
        var remaining = Assert.Single(node.Children);
        Assert.Same(second, remaining);
    }

    [Fact]
    public void TryRemoveChildren_WithoutOutParameterReturnsFalseWhenNothingMatches()
    {
        var child = new YamlScalarNode("keep");
        var node = new YamlSequenceNode(child);

        var removed = node.TryRemoveChildren<YamlScalarNode>(m => m.Value == "remove");

        Assert.False(removed);
        var remaining = Assert.Single(node.Children);
        Assert.Same(child, remaining);
    }
}
