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
    public void GetChild_ReturnsMatchedScalarChild()
    {
        var root = ReadYaml();

        var child = root.GetChild<YamlScalarNode>("receipt");

        Assert.NotNull(child);
        Assert.Equal("Oz-Ware Purchase Invoice", child.Value);
    }

    [Fact]
    public void GetChild_ReturnsNullWhenKeyDoesNotExist()
    {
        var root = ReadYaml();

        var child = root.GetChild<YamlScalarNode>("non-exist");

        Assert.Null(child);
    }

    [Fact]
    public void GetChild_ReturnsComplexNodeWhenRequestedTypeMatches()
    {
        var root = ReadYaml();

        var child = root.GetChild<YamlMappingNode>("customer");

        Assert.NotNull(child);
        Assert.Equal("Dorothy", child.GetChild<YamlScalarNode>("first_name")!.Value);
    }

    [Fact]
    public void GetChild_ReturnsNullWhenExistingChildHasDifferentType()
    {
        var root = ReadYaml();

        var child = root.GetChild<YamlSequenceNode>("customer");

        Assert.Null(child);
    }

    [Fact]
    public void GetRequiredChild_ThrowsWhenExistingChildHasDifferentType()
    {
        var root = ReadYaml();

        var exception = Assert.Throws<InvalidOperationException>(() => root.GetRequiredChild<YamlSequenceNode>("customer"));

        Assert.Contains("customer", exception.Message);
        Assert.Contains(nameof(YamlSequenceNode), exception.Message);
    }
    
    [Fact]
    public void GetRequiredChild_ReturnsMatchedChild()
    {
        var root = ReadYaml();

        var child = root.GetRequiredChild<YamlScalarNode>("date");

        Assert.Equal("2012-08-06", child.Value);
    }

    [Fact]
    public void GetRequiredChild_ThrowsWhenKeyDoesNotExist()
    {
        var root = ReadYaml();

        var exception = Assert.Throws<KeyNotFoundException>(() => root.GetRequiredChild<YamlScalarNode>("non-exist"));
        Assert.Contains("non-exist", exception.Message);
    }

    [Fact]
    public void AddScalarChild_AppendsScalarChildByDefault()
    {
        var node = new YamlMappingNode();

        var result = node.AddScalarChild("name", "Dorothy");

        Assert.Same(node, result);
        Assert.Equal("Dorothy", node.GetRequiredChild<YamlScalarNode>("name").Value);
    }

    [Fact]
    public void AddScalarChild_InsertsScalarChildAtRequestedIndex()
    {
        var node = new YamlMappingNode
        {
            { "first", "1" },
            { "third", "3" },
        };

        node.AddScalarChild("second", "2", 1);

        Assert.Equal(new[] { "first", "second", "third" }, node.GetChildren().Select(m => m.Key.Value).ToArray());
    }

    [Fact]
    public void RemoveChild_RemovesMatchedChildAndReturnsSameNode()
    {
        var root = ReadYaml();

        var result = root.RemoveChild("receipt");

        Assert.Same(root, result);
        Assert.Null(root.GetChild<YamlScalarNode>("receipt"));
    }

    [Fact]
    public void RemoveChild_DoesNothingWhenKeyDoesNotExist()
    {
        var root = ReadYaml();
        var before = root.GetChildren().Select(m => m.Key.Value).ToArray();

        root.RemoveChild("non-exist");

        Assert.Equal(before, root.GetChildren().Select(m => m.Key.Value).ToArray());
    }
    
    [Fact]
    public void GetChildren_ReturnsScalarKeyYamlNodePairsInDocumentOrder()
    {
        var root = ReadYaml();

        var children = root.GetChildren().ToArray();

        string[] keys = ["receipt", "date", "enabled", "customer", "items", "bill-to", "ship-to", "specialDelivery"];
        Assert.Equal(keys, children.Select(m => m.Key.Value!).ToArray());
        Assert.All(children, m => Assert.IsAssignableFrom<YamlNode>(m.Value));
    }

    [Fact]
    public void GetChildren_AppliesFilterBeforeCasting()
    {
        var root = ReadYaml();

        var children = root.GetChildren<YamlMappingNode>((key, _) => key.IsScalarValue("customer")).ToArray();

        var child = Assert.Single(children);
        Assert.Equal("customer", child.Key.Value);
        Assert.Equal("Gale", child.Value.GetRequiredChild<YamlScalarNode>("family_name").Value);
    }

    [Fact]
    public void GetChildren_ThrowsWhenUnfilteredChildCannotBeCast()
    {
        var root = ReadYaml();

        var exception = Assert.ThrowsAny<Exception>(() => root.GetChildren<YamlMappingNode>().ToArray());

        Assert.Equal("Microsoft.CSharp.RuntimeBinder.RuntimeBinderException", exception.GetType().FullName);
    }

    [Fact]
    public void GetChildren_WithKeyCollectionReturnsOnlyMatchingKeys()
    {
        var root = ReadYaml();

        var children = root.GetChildren<YamlNode>(["date", "enabled", "missing"]).ToArray();

        Assert.Equal(new[] { "date", "enabled" }, children.Select(m => m.Key.Value).ToArray());
        Assert.Equal(new[] { "2012-08-06", "true" }, children.Select(m => ((YamlScalarNode)m.Value).Value).ToArray());
    }

    [Fact]
    public void SetScalarChild_AddsScalarWhenKeyDoesNotExist()
    {
        var node = new YamlMappingNode();

        var (child, changed) = node.SetScalarChild("name", "Dorothy", ScalarStyle.DoubleQuoted);

        Assert.NotNull(child);
        Assert.True(changed);
        Assert.Same(child, node.GetRequiredChild<YamlScalarNode>("name"));
        Assert.Equal("Dorothy", child.Value);
        Assert.Equal(ScalarStyle.DoubleQuoted, child.Style);
    }

    [Fact]
    public void SetScalarChild_ReturnsExistingScalarWithoutChangeWhenValueAndStyleMatch()
    {
        var node = new YamlMappingNode();
        var (original, _) = node.SetScalarChild("name", "Dorothy", ScalarStyle.SingleQuoted);

        var (child, changed) = node.SetScalarChild("name", "Dorothy", ScalarStyle.SingleQuoted);

        Assert.NotNull(original);
        Assert.NotNull(child);
        Assert.False(changed);
        Assert.Same(original, child);
        Assert.Equal(ScalarStyle.SingleQuoted, child.Style);
    }

    [Fact]
    public void SetScalarChild_UpdatesExistingScalarValueAndKeepsStyleWhenStyleIsNotSpecified()
    {
        var node = new YamlMappingNode();
        var (original, _) = node.SetScalarChild("name", "Dorothy", ScalarStyle.DoubleQuoted);

        var (child, changed) = node.SetScalarChild("name", "Gale");

        Assert.NotNull(original);
        Assert.NotNull(child);
        Assert.True(changed);
        Assert.Same(original, child);
        Assert.Equal("Gale", child.Value);
        Assert.Equal(ScalarStyle.DoubleQuoted, child.Style);
    }

    [Fact]
    public void SetScalarChild_UpdatesExistingScalarStyleWithoutChangingValue()
    {
        var node = new YamlMappingNode();
        var (original, _) = node.SetScalarChild("name", "Dorothy", ScalarStyle.Plain);

        var (child, changed) = node.SetScalarChild("name", "Dorothy", ScalarStyle.DoubleQuoted);

        Assert.NotNull(original);
        Assert.NotNull(child);
        Assert.True(changed);
        Assert.Same(original, child);
        Assert.Equal("Dorothy", child.Value);
        Assert.Equal(ScalarStyle.DoubleQuoted, child.Style);
    }

    [Fact]
    public void SetScalarChild_ReplacesNonScalarValueWhenTypeMismatchIsAllowed()
    {
        var node = new YamlMappingNode
        {
            { new YamlScalarNode("name"), new YamlMappingNode() },
        };

        var (child, changed) = node.SetScalarChild("name", "Dorothy");

        Assert.NotNull(child);
        Assert.True(changed);
        Assert.Equal("Dorothy", child.Value);
        Assert.Same(child, node.GetRequiredChild<YamlScalarNode>("name"));
    }

    [Fact]
    public void SetScalarChild_ThrowsForNonScalarValueWhenTypeMismatchThrows()
    {
        var node = new YamlMappingNode
        {
            { new YamlScalarNode("name"), new YamlMappingNode() },
        };

        var exception = Assert.Throws<InvalidOperationException>(() => node.SetScalarChild("name", "Dorothy", conflictBehavior: YamlScalarChildConflictBehavior.Throw));

        Assert.Contains("name", exception.Message);
        Assert.Contains(nameof(YamlScalarNode), exception.Message);
    }

    [Fact]
    public void SetScalarChild_IgnoresNonScalarValueWhenConflictBehaviorIsIgnore()
    {
        var existing = new YamlMappingNode();
        var node = new YamlMappingNode
        {
            { new YamlScalarNode("name"), existing },
        };

        var (child, changed) = node.SetScalarChild("name", "Dorothy", conflictBehavior: YamlScalarChildConflictBehavior.Ignore);

        Assert.Null(child);
        Assert.False(changed);
        Assert.Same(existing, node.GetRequiredChild<YamlMappingNode>("name"));
    }

    [Fact]
    public void SetScalarChild_BoolWritesLowercasePlainScalar()
    {
        var node = new YamlMappingNode();

        var (child, changed) = node.SetScalarChild("enabled", true);

        Assert.NotNull(child);
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
        Assert.Same(created, node.GetRequiredChild<YamlSequenceNode>("items"));
    }

    [Fact]
    public void GetOrAddChild_AddsNewDefaultNodeWhenKeyDoesNotExist()
    {
        var node = new YamlMappingNode();

        var (child, added) = node.GetOrAddChild<YamlMappingNode>("customer");

        Assert.True(added);
        Assert.Same(child, node.GetRequiredChild<YamlMappingNode>("customer"));
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

    [Fact]
    public void TryRemoveChildren_RemovesAllMatchedChildren()
    {
        var node = new YamlMappingNode
        {
            { "first", "remove" },
            { "second", "keep" },
            { "third", "remove" },
        };

        var removed = node.TryRemoveChildren<YamlScalarNode>((_, value) => value.Value == "remove", out var removedNodes);

        Assert.True(removed);
        Assert.Equal(new[] { "remove", "remove" }, removedNodes.Select(m => m.Value).ToArray());
        Assert.Equal(new[] { "second" }, node.GetChildren().Select(m => m.Key.Value).ToArray());
    }

    [Fact]
    public void TryRemoveChildren_WhenNothingMatches_ShouldReturnFalse()
    {
        var node = new YamlMappingNode
        {
            { "name", "Dorothy" },
        };

        var removed = node.TryRemoveChildren<YamlScalarNode>((_, value) => value.Value == "missing", out var removedNodes);

        Assert.False(removed);
        Assert.Empty(removedNodes);
        Assert.Equal("Dorothy", node.GetRequiredChild<YamlScalarNode>("name").Value);
    }

    [Fact]
    public void MoveChildByKeyNode_MovesChildAtKeyNodeToDestinationIndex()
    {
        var firstKey = new YamlScalarNode("first");
        var secondKey = new YamlScalarNode("second");
        var thirdKey = new YamlScalarNode("third");
        var node = new YamlMappingNode
        {
            { firstKey, new YamlScalarNode("1") },
            { secondKey, new YamlScalarNode("2") },
            { thirdKey, new YamlScalarNode("3") },
        };

        var moved = node.MoveChildByKeyNode(thirdKey, 0);

        Assert.True(moved);
        Assert.Equal(new[] { "third", "first", "second" }, node.GetChildren().Select(m => m.Key.Value).ToArray());
    }

    [Fact]
    public void MoveChildByValueNode_MovesChildAtValueNodeToDestinationIndex()
    {
        var firstValue = new YamlScalarNode("1");
        var secondValue = new YamlScalarNode("2");
        var thirdValue = new YamlScalarNode("3");
        var node = new YamlMappingNode
        {
            { new YamlScalarNode("first"), firstValue },
            { new YamlScalarNode("second"), secondValue },
            { new YamlScalarNode("third"), thirdValue },
        };

        var moved = node.MoveChildByValueNode(thirdValue, 0);

        Assert.True(moved);
        Assert.Equal(new[] { "third", "first", "second" }, node.GetChildren().Select(m => m.Key.Value).ToArray());
    }

    [Fact]
    public void MoveChildByKey_MovesChildAtScalarKeyToDestinationIndex()
    {
        var node = new YamlMappingNode
        {
            { "first", "1" },
            { "second", "2" },
            { "third", "3" },
        };

        var moved = node.MoveChildByKey("third", 0);

        Assert.True(moved);
        Assert.Equal(new[] { "third", "first", "second" }, node.GetChildren().Select(m => m.Key.Value).ToArray());
    }
}
