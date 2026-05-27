namespace FclEx.YamlDotNet;

public class YamlNodeExtensionsTests
{
    [Fact]
    public void IsScalarWithValue_ReturnsTrueForMatchingScalarValue()
    {
        var node = new YamlScalarNode("name");

        var result = node.IsScalarWithValue("name");

        Assert.True(result);
    }

    [Fact]
    public void IsScalarWithValue_ReturnsFalseForDifferentScalarValue()
    {
        var node = new YamlScalarNode("name");

        var result = node.IsScalarWithValue("other");

        Assert.False(result);
    }

    [Fact]
    public void IsScalarWithValue_ReturnsFalseForNonScalarNode()
    {
        var node = new YamlMappingNode();

        var result = node.IsScalarWithValue("name");

        Assert.False(result);
    }

    [Fact]
    public void IsScalarWithValue_MatchesNullScalarValue()
    {
        var node = new YamlScalarNode(null);

        var result = node.IsScalarWithValue(null!);

        Assert.True(result);
    }
}
