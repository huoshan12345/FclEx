namespace FclEx.YamlDotNet;

public class YamlMappingNodeExtensionsTests
{
    public static readonly string Yaml = """
                                         ---
                                         receipt:     Oz-Ware Purchase Invoice
                                         date:        2012-08-06
                                         enabled:     true
                                         customer:
                                             first_name:   Dorothy
                                             family_name:  Gale

                                         items:
                                             - part_no:   A4786
                                               descrip:   Water Bucket (Filled)
                                               price:     1.47
                                               quantity:  4

                                             - part_no:   E1628
                                               descrip:   High Heeled "Ruby" Slippers
                                               size:      8
                                               price:     133.7
                                               quantity:  1

                                         bill-to:  &id001
                                             street: |
                                                     123 Tornado Alley
                                                     Suite 16
                                             city:   East Centerville
                                             state:  KS

                                         ship-to:  *id001

                                         specialDelivery:  >
                                             Follow the Yellow Brick
                                             Road to the Emerald City.
                                             Pay no attention to the
                                             man behind the curtain.
                                         ...
                                         """;

    private static YamlMappingNode ReadYaml()
    {
        using var input = new StringReader(Yaml);
        var yaml = new YamlStream();
        yaml.Load(input);
        var root = yaml.Documents[0].RootNode.CastTo<YamlMappingNode>();
        return root;
    }

    [Fact]
    public void Child_ReturnsMatchedScalarChild()
    {
        var root = ReadYaml();

        var child = root.Child<YamlScalarNode>("receipt");

        Assert.NotNull(child);
        Assert.Equal("Oz-Ware Purchase Invoice", child.Value);
    }

    [Fact]
    public void Child_ReturnsNullWhenKeyDoesNotExist()
    {
        var root = ReadYaml();

        var child = root.Child<YamlScalarNode>("non-exist");

        Assert.Null(child);
    }

    [Fact]
    public void Child_ReturnsComplexNodeWhenRequestedTypeMatches()
    {
        var root = ReadYaml();

        var child = root.Child<YamlMappingNode>("customer");

        Assert.NotNull(child);
        Assert.Equal("Dorothy", child.Child<YamlScalarNode>("first_name")!.Value);
    }

    [Fact]
    public void Child_ThrowsWhenExistingChildHasDifferentType()
    {
        var root = ReadYaml();

        var exception = Assert.ThrowsAny<Exception>(() => root.Child<YamlSequenceNode>("customer"));

        Assert.Equal("Microsoft.CSharp.RuntimeBinder.RuntimeBinderException", exception.GetType().FullName);
    }
    
    [Fact]
    public void RequiredChild_ReturnsMatchedChild()
    {
        var root = ReadYaml();

        var child = root.RequiredChild<YamlScalarNode>("date");

        Assert.Equal("2012-08-06", child.Value);
    }

    [Fact]
    public void RequiredChild_ThrowsWhenKeyDoesNotExist()
    {
        var root = ReadYaml();

        var exception = Assert.Throws<KeyNotFoundException>(() => root.RequiredChild<YamlScalarNode>("non-exist"));
        Assert.Contains("YamlScalarNode", exception.Message);
        Assert.Contains("non-exist", exception.Message);
    }

    [Fact]
    public void AddChild_AppendsScalarChildByDefault()
    {
        var node = new YamlMappingNode();

        var result = node.AddChild("name", "Dorothy");

        Assert.Same(node, result);
        Assert.Equal("Dorothy", node.RequiredChild<YamlScalarNode>("name").Value);
    }

    [Fact]
    public void AddChild_InsertsScalarChildAtRequestedIndex()
    {
        var node = new YamlMappingNode
        {
            { "first", "1" },
            { "third", "3" },
        };

        node.AddChild("second", "2", 1);

        Assert.Equal(new[] { "first", "second", "third" }, node.Children().Select(m => m.Key.Value).ToArray());
    }

    [Fact]
    public void RemoveChild_RemovesMatchedChildAndReturnsSameNode()
    {
        var root = ReadYaml();

        var result = root.RemoveChild("receipt");

        Assert.Same(root, result);
        Assert.Null(root.Child<YamlScalarNode>("receipt"));
    }

    [Fact]
    public void RemoveChild_DoesNothingWhenKeyDoesNotExist()
    {
        var root = ReadYaml();
        var before = root.Children().Select(m => m.Key.Value).ToArray();

        root.RemoveChild("non-exist");

        Assert.Equal(before, root.Children().Select(m => m.Key.Value).ToArray());
    }
    
    [Fact]
    public void Children_ReturnsScalarKeyYamlNodePairsInDocumentOrder()
    {
        var root = ReadYaml();

        var children = root.Children().ToArray();

        string[] keys = ["receipt", "date", "enabled", "customer", "items", "bill-to", "ship-to", "specialDelivery"];
        Assert.Equal(keys, children.Select(m => m.Key.Value!).ToArray());
        Assert.All(children, m => Assert.IsAssignableFrom<YamlNode>(m.Value));
    }

    [Fact]
    public void Children_AppliesFilterBeforeCasting()
    {
        var root = ReadYaml();

        var children = root.Children<YamlMappingNode>((key, _) => key.IsScalar("customer")).ToArray();

        var child = Assert.Single(children);
        Assert.Equal("customer", child.Key.Value);
        Assert.Equal("Gale", child.Value.RequiredChild<YamlScalarNode>("family_name").Value);
    }

    [Fact]
    public void Children_ThrowsWhenUnfilteredChildCannotBeCast()
    {
        var root = ReadYaml();

        var exception = Assert.ThrowsAny<Exception>(() => root.Children<YamlMappingNode>().ToArray());

        Assert.Equal("Microsoft.CSharp.RuntimeBinder.RuntimeBinderException", exception.GetType().FullName);
    }

    [Fact]
    public void Children_WithKeyCollectionReturnsOnlyMatchingKeys()
    {
        var root = ReadYaml();

        var children = root.Children<YamlScalarNode, YamlNode>(["date", "enabled", "missing"]).ToArray();

        Assert.Equal(new[] { "date", "enabled" }, children.Select(m => m.Key.Value).ToArray());
        Assert.Equal(new[] { "2012-08-06", "true" }, children.Select(m => ((YamlScalarNode)m.Value).Value).ToArray());
    }

    [Fact]
    public void AddOrUpdateChild_AddsScalarWhenKeyDoesNotExist()
    {
        var node = new YamlMappingNode();

        var (child, changed) = node.AddOrUpdateChild("name", "Dorothy", ScalarStyle.DoubleQuoted);

        Assert.True(changed);
        Assert.Same(child, node.RequiredChild<YamlScalarNode>("name"));
        Assert.Equal("Dorothy", child.Value);
        Assert.Equal(ScalarStyle.DoubleQuoted, child.Style);
    }

    [Fact]
    public void AddOrUpdateChild_ReturnsExistingScalarWithoutChangeWhenValueAndStyleMatch()
    {
        var node = new YamlMappingNode();
        var (original, _) = node.AddOrUpdateChild("name", "Dorothy", ScalarStyle.SingleQuoted);

        var (child, changed) = node.AddOrUpdateChild("name", "Dorothy", ScalarStyle.SingleQuoted);

        Assert.False(changed);
        Assert.Same(original, child);
        Assert.Equal(ScalarStyle.SingleQuoted, child.Style);
    }

    [Fact]
    public void AddOrUpdateChild_UpdatesExistingScalarValueAndKeepsStyleWhenStyleIsNotSpecified()
    {
        var node = new YamlMappingNode();
        var (original, _) = node.AddOrUpdateChild("name", "Dorothy", ScalarStyle.DoubleQuoted);

        var (child, changed) = node.AddOrUpdateChild("name", "Gale");

        Assert.True(changed);
        Assert.Same(original, child);
        Assert.Equal("Gale", child.Value);
        Assert.Equal(ScalarStyle.DoubleQuoted, child.Style);
    }

    [Fact]
    public void AddOrUpdateChild_UpdatesExistingScalarStyleWithoutChangingValue()
    {
        var node = new YamlMappingNode();
        var (original, _) = node.AddOrUpdateChild("name", "Dorothy", ScalarStyle.Plain);

        var (child, changed) = node.AddOrUpdateChild("name", "Dorothy", ScalarStyle.DoubleQuoted);

        Assert.True(changed);
        Assert.Same(original, child);
        Assert.Equal("Dorothy", child.Value);
        Assert.Equal(ScalarStyle.DoubleQuoted, child.Style);
    }

    [Fact]
    public void AddOrUpdateChild_ReplacesNonScalarValueWhenTypeMismatchIsAllowed()
    {
        var node = new YamlMappingNode
        {
            { new YamlScalarNode("name"), new YamlMappingNode() },
        };

        var (child, changed) = node.AddOrUpdateChild("name", "Dorothy");

        Assert.True(changed);
        Assert.Equal("Dorothy", child.Value);
        Assert.Same(child, node.RequiredChild<YamlScalarNode>("name"));
    }

    [Fact]
    public void AddOrUpdateChild_ThrowsForNonScalarValueWhenTypeMismatchThrows()
    {
        var node = new YamlMappingNode
        {
            { new YamlScalarNode("name"), new YamlMappingNode() },
        };

        var exception = Assert.Throws<InvalidOperationException>(() => node.AddOrUpdateChild("name", "Dorothy", throwOnTypeMismatch: true));

        Assert.Contains("name", exception.Message);
        Assert.Contains(nameof(YamlScalarNode), exception.Message);
    }

    [Fact]
    public void AddOrUpdateChild_BoolWritesLowercasePlainScalar()
    {
        var node = new YamlMappingNode();

        var (child, changed) = node.AddOrUpdateChild("enabled", true);

        Assert.True(changed);
        Assert.Equal("true", child.Value);
        Assert.Equal(ScalarStyle.Plain, child.Style);
    }

    [Fact]
    public void GetOrAddChild_ReturnsExistingChildWithoutInvokingFactory()
    {
        var existing = new YamlMappingNode();
        var node = new YamlMappingNode
        {
            { new YamlScalarNode("customer"), existing },
        };

        var (child, added) = node.GetOrAddChild<YamlMappingNode>("customer", () => throw new InvalidOperationException("Factory should not be invoked."));

        Assert.False(added);
        Assert.Same(existing, child);
    }

    [Fact]
    public void GetOrAddChild_AddsFactoryResultWhenKeyDoesNotExist()
    {
        var node = new YamlMappingNode();
        var created = new YamlSequenceNode();

        var (child, added) = node.GetOrAddChild("items", () => created);

        Assert.True(added);
        Assert.Same(created, child);
        Assert.Same(created, node.RequiredChild<YamlSequenceNode>("items"));
    }

    [Fact]
    public void GetOrAddChild_AddsNewDefaultNodeWhenKeyDoesNotExist()
    {
        var node = new YamlMappingNode();

        var (child, added) = node.GetOrAddChild<YamlMappingNode>("customer");

        Assert.True(added);
        Assert.Same(child, node.RequiredChild<YamlMappingNode>("customer"));
    }

    [Fact]
    public void GetOrAddChild_ThrowsWhenExistingChildHasDifferentType()
    {
        var node = new YamlMappingNode
        {
            { "customer", "Dorothy" },
        };

        var exception = Assert.Throws<InvalidOperationException>(() => node.GetOrAddChild<YamlMappingNode>("customer"));

        Assert.Contains("customer", exception.Message);
        Assert.Contains(nameof(YamlMappingNode), exception.Message);
    }
}
