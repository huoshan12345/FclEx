namespace FclEx.YamlDotNet;

public class YamlNodeExtensionsTests
{
    [Fact]
    public void IsScalarValue_ReturnsTrueForMatchingScalarValue()
    {
        var node = new YamlScalarNode("name");

        var result = node.IsScalarValue("name");

        Assert.True(result);
    }

    [Fact]
    public void IsScalarValue_ReturnsFalseForDifferentScalarValue()
    {
        var node = new YamlScalarNode("name");

        var result = node.IsScalarValue("other");

        Assert.False(result);
    }

    [Fact]
    public void IsScalarValue_ReturnsFalseForNonScalarNode()
    {
        var node = new YamlMappingNode();

        var result = node.IsScalarValue("name");

        Assert.False(result);
    }

    [Fact]
    public void IsScalarValue_MatchesNullScalarValue()
    {
        var node = new YamlScalarNode(null);

        var result = node.IsScalarValue(null!);

        Assert.True(result);
    }
}
