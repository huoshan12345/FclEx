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
    public void GetChild_WithYamlNodeKeyReturnsMatchedChild()
    {
        var key = new YamlMappingNode { { "kind", "name" } };
        var value = new YamlScalarNode("Dorothy");
        var node = new YamlMappingNode
        {
            { key, value },
        };

        var child = node.GetChild<YamlScalarNode>(key);

        Assert.Same(value, child);
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
    public void GetRequiredChild_WithYamlNodeKeyThrowsWhenKeyDoesNotExist()
    {
        var key = new YamlMappingNode { { "kind", "missing" } };
        var node = new YamlMappingNode();

        var exception = Assert.Throws<KeyNotFoundException>(() => node.GetRequiredChild<YamlScalarNode>(key));

        Assert.Contains(nameof(YamlMappingNode), exception.Message);
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
    public void AddScalarChild_ThrowsWhenIndexIsOutOfRange()
    {
        var node = new YamlMappingNode();

        Assert.Throws<ArgumentOutOfRangeException>(() => node.AddScalarChild("name", "Dorothy", 1));
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
    public void RemoveChild_WithYamlNodeKeyRemovesMatchedChild()
    {
        var key = new YamlMappingNode { { "kind", "name" } };
        var node = new YamlMappingNode
        {
            { key, new YamlScalarNode("Dorothy") },
        };

        node.RemoveChild(key);

        Assert.Empty(node.Children);
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

        var children = root.GetChildren<YamlMappingNode>((key, _) => key.IsScalarWithValue("customer")).ToArray();

        var child = Assert.Single(children);
        Assert.Equal("customer", child.Key.Value);
        Assert.Equal("Gale", child.Value.GetRequiredChild<YamlScalarNode>("family_name").Value);
    }

    [Fact]
    public void GetChildren_ReturnsOnlyChildrenMatchingRequestedValueType()
    {
        var root = ReadYaml();

        var children = root.GetChildren<YamlMappingNode>().ToArray();

        Assert.Equal(new[] { "customer", "bill-to", "ship-to" }, children.Select(m => m.Key.Value).ToArray());
        Assert.All(children, m => Assert.IsType<YamlMappingNode>(m.Value));
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
    public void GetChildren_WithEmptyKeyCollectionReturnsEmptySequence()
    {
        var root = ReadYaml();

        var children = root.GetChildren<YamlNode>([]).ToArray();

        Assert.Empty(children);
    }

    [Fact]
    public void GetChildren_WithDuplicateKeysReturnsEachMatchingChildOnce()
    {
        var root = ReadYaml();

        var children = root.GetChildren<YamlNode>(["date", "date"]).ToArray();

        var child = Assert.Single(children);
        Assert.Equal("date", child.Key.Value);
    }

    [Fact]
    public void TrySetScalarChild_AddsScalarWhenKeyDoesNotExist()
    {
        var node = new YamlMappingNode();

        var (child, changed) = node.TrySetScalarChild("name", "Dorothy", ScalarStyle.DoubleQuoted);

        Assert.NotNull(child);
        Assert.True(changed);
        Assert.Same(child, node.GetRequiredChild<YamlScalarNode>("name"));
        Assert.Equal("Dorothy", child.Value);
        Assert.Equal(ScalarStyle.DoubleQuoted, child.Style);
    }

    [Fact]
    public void TrySetScalarChild_ReturnsExistingScalarWithoutChangeWhenValueAndStyleMatch()
    {
        var node = new YamlMappingNode();
        var (original, _) = node.TrySetScalarChild("name", "Dorothy", ScalarStyle.SingleQuoted);

        var (child, changed) = node.TrySetScalarChild("name", "Dorothy", ScalarStyle.SingleQuoted);

        Assert.NotNull(original);
        Assert.NotNull(child);
        Assert.False(changed);
        Assert.Same(original, child);
        Assert.Equal(ScalarStyle.SingleQuoted, child.Style);
    }

    [Fact]
    public void TrySetScalarChild_UpdatesExistingScalarValueAndKeepsStyleWhenStyleIsNotSpecified()
    {
        var node = new YamlMappingNode();
        var (original, _) = node.TrySetScalarChild("name", "Dorothy", ScalarStyle.DoubleQuoted);

        var (child, changed) = node.TrySetScalarChild("name", "Gale");

        Assert.NotNull(original);
        Assert.NotNull(child);
        Assert.True(changed);
        Assert.Same(original, child);
        Assert.Equal("Gale", child.Value);
        Assert.Equal(ScalarStyle.DoubleQuoted, child.Style);
    }

    [Fact]
    public void TrySetScalarChild_UpdatesExistingScalarStyleWithoutChangingValue()
    {
        var node = new YamlMappingNode();
        var (original, _) = node.TrySetScalarChild("name", "Dorothy", ScalarStyle.Plain);

        var (child, changed) = node.TrySetScalarChild("name", "Dorothy", ScalarStyle.DoubleQuoted);

        Assert.NotNull(original);
        Assert.NotNull(child);
        Assert.True(changed);
        Assert.Same(original, child);
        Assert.Equal("Dorothy", child.Value);
        Assert.Equal(ScalarStyle.DoubleQuoted, child.Style);
    }

    [Fact]
    public void TrySetScalarChild_ReplacesNonScalarValueWhenConflictBehaviorIsReplace()
    {
        var node = new YamlMappingNode
        {
            { new YamlScalarNode("name"), new YamlMappingNode() },
        };

        var (child, changed) = node.TrySetScalarChild("name", "Dorothy", conflictBehavior: YamlScalarChildConflictBehavior.Replace);

        Assert.NotNull(child);
        Assert.True(changed);
        Assert.Equal("Dorothy", child.Value);
        Assert.Same(child, node.GetRequiredChild<YamlScalarNode>("name"));
    }

    [Fact]
    public void TrySetScalarChild_ThrowsForNonScalarValueWhenConflictBehaviorIsThrow()
    {
        var node = new YamlMappingNode
        {
            { new YamlScalarNode("name"), new YamlMappingNode() },
        };

        var exception = Assert.Throws<InvalidOperationException>(() => node.TrySetScalarChild("name", "Dorothy", conflictBehavior: YamlScalarChildConflictBehavior.Throw));

        Assert.Contains("name", exception.Message);
        Assert.Contains(nameof(YamlScalarNode), exception.Message);
    }

    [Fact]
    public void TrySetScalarChild_IgnoresNonScalarValueByDefault()
    {
        var existing = new YamlMappingNode();
        var node = new YamlMappingNode
        {
            { new YamlScalarNode("name"), existing },
        };

        var (child, changed) = node.TrySetScalarChild("name", "Dorothy");

        Assert.Null(child);
        Assert.False(changed);
        Assert.Same(existing, node.GetRequiredChild<YamlMappingNode>("name"));
    }

    [Fact]
    public void TrySetScalarChild_BoolWritesLowercasePlainScalar()
    {
        var node = new YamlMappingNode();

        var (child, changed) = node.TrySetScalarChild("enabled", true);

        Assert.NotNull(child);
        Assert.True(changed);
        Assert.Equal("true", child.Value);
        Assert.Equal(ScalarStyle.Plain, child.Style);
    }

    [Fact]
    public void TrySetScalarChild_BoolWritesFalseAsLowercasePlainScalar()
    {
        var node = new YamlMappingNode();

        var (child, changed) = node.TrySetScalarChild("enabled", false);

        Assert.NotNull(child);
        Assert.True(changed);
        Assert.Equal("false", child.Value);
        Assert.Equal(ScalarStyle.Plain, child.Style);
    }

    [Fact]
    public void SetScalarChild_ReplacesNonScalarValueByDefault()
    {
        var node = new YamlMappingNode
        {
            { new YamlScalarNode("name"), new YamlMappingNode() },
        };

        var (child, changed) = node.SetScalarChild("name", "Dorothy");

        Assert.True(changed);
        Assert.Equal("Dorothy", child.Value);
        Assert.Same(child, node.GetRequiredChild<YamlScalarNode>("name"));
    }

    [Fact]
    public void SetScalarChild_BoolWritesLowercasePlainScalar()
    {
        var node = new YamlMappingNode();

        var (child, changed) = node.SetScalarChild("enabled", true);

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
    public void TryRemoveChildren_IgnoresNonScalarKeysAndDifferentValueTypes()
    {
        var nonScalarKey = new YamlSequenceNode(new YamlScalarNode("name"));
        var scalarValue = new YamlScalarNode("Dorothy");
        var mappingValue = new YamlMappingNode();
        var node = new YamlMappingNode
        {
            { nonScalarKey, scalarValue },
            { new YamlScalarNode("name"), mappingValue },
        };

        var removed = node.TryRemoveChildren<YamlScalarNode>((_, value) => value.Value == "Dorothy", out var removedNodes);

        Assert.False(removed);
        Assert.Empty(removedNodes);
        Assert.Equal(2, node.Children.Count);
    }

    [Fact]
    public void TryRemoveChildren_WithKeyRemovesMatchingTypedChildren()
    {
        var node = new YamlMappingNode
        {
            { "name", "Dorothy" },
            { "customer", new YamlMappingNode() },
        };

        var removed = node.TryRemoveChildren<YamlScalarNode>("name", out var removedNodes);

        Assert.True(removed);
        Assert.Equal("Dorothy", Assert.Single(removedNodes).Value);
        Assert.Null(node.GetChild<YamlScalarNode>("name"));
        Assert.NotNull(node.GetChild<YamlMappingNode>("customer"));
    }

    [Fact]
    public void TryRemoveChildren_WithKeyReturnsFalseWhenValueTypeDoesNotMatch()
    {
        var node = new YamlMappingNode
        {
            { "customer", new YamlMappingNode() },
        };

        var removed = node.TryRemoveChildren<YamlScalarNode>("customer", out var removedNodes);

        Assert.False(removed);
        Assert.Empty(removedNodes);
        Assert.NotNull(node.GetChild<YamlMappingNode>("customer"));
    }

    [Fact]
    public void TryRemoveChildren_WithKeyOnlyReturnsWhetherChildWasRemoved()
    {
        var node = new YamlMappingNode
        {
            { "name", "Dorothy" },
        };

        var removed = node.TryRemoveChildren("name");

        Assert.True(removed);
        Assert.Empty(node.Children);
    }

    [Fact]
    public void TryRemoveChildren_WithKeyOnlyReturnsFalseWhenKeyDoesNotExist()
    {
        var node = new YamlMappingNode
        {
            { "name", "Dorothy" },
        };

        var removed = node.TryRemoveChildren("missing");

        Assert.False(removed);
        Assert.Single(node.Children);
    }

    [Fact]
    public void FindChild_WithPredicateReturnsFirstMatchedChildAndIndex()
    {
        var root = ReadYaml();

        var (key, value, index) = root.FindChild<YamlScalarNode>((k, v) => k.IsScalarWithValue("date") && v.Value == "2012-08-06");

        Assert.NotNull(key);
        Assert.NotNull(value);
        Assert.Equal("date", ((YamlScalarNode)key).Value);
        Assert.Equal("2012-08-06", value.Value);
        Assert.Equal(1, index);
    }

    [Fact]
    public void FindChild_WithPredicateIgnoresValuesOfDifferentType()
    {
        var root = ReadYaml();

        var (key, value, index) = root.FindChild<YamlMappingNode>((_, _) => true);

        Assert.NotNull(key);
        Assert.NotNull(value);
        Assert.Equal("customer", ((YamlScalarNode)key).Value);
        Assert.Equal(3, index);
    }

    [Fact]
    public void FindChild_WithPredicateReturnsNullsAndMinusOneWhenNoChildMatches()
    {
        var root = ReadYaml();

        var (key, value, index) = root.FindChild<YamlScalarNode>((_, v) => v.Value == "missing");

        Assert.Null(key);
        Assert.Null(value);
        Assert.Equal(-1, index);
    }

    [Fact]
    public void FindChild_WithYamlNodeKeyReturnsMatchedChild()
    {
        var key = new YamlMappingNode { { "kind", "name" } };
        var value = new YamlScalarNode("Dorothy");
        var node = new YamlMappingNode
        {
            { key, value },
        };

        var (foundKey, foundValue, index) = node.FindChild<YamlScalarNode>(key);

        Assert.Same(key, foundKey);
        Assert.Same(value, foundValue);
        Assert.Equal(0, index);
    }

    [Fact]
    public void FindChild_WithStringKeyReturnsMatchedScalarKey()
    {
        var root = ReadYaml();

        var (key, value, index) = root.FindChild<YamlScalarNode>("receipt");

        Assert.NotNull(key);
        Assert.NotNull(value);
        Assert.Equal("receipt", key.Value);
        Assert.Equal("Oz-Ware Purchase Invoice", value.Value);
        Assert.Equal(0, index);
    }

    [Fact]
    public void FindChild_WithStringKeyReturnsNullsWhenValueTypeDoesNotMatch()
    {
        var root = ReadYaml();

        var (key, value, index) = root.FindChild<YamlSequenceNode>("receipt");

        Assert.Null(key);
        Assert.Null(value);
        Assert.Equal(-1, index);
    }

    [Fact]
    public void FindChildren_WithPredicateReturnsAllMatchedChildren()
    {
        var root = ReadYaml();

        var children = root.FindChildren<YamlScalarNode>((_, _) => true);

        Assert.Equal(new[] { 0, 1, 2, 7 }, children.Select(m => m.Index).ToArray());
        Assert.Equal(new[] { "receipt", "date", "enabled", "specialDelivery" }, children.Select(m => ((YamlScalarNode)m.Key).Value).ToArray());
    }

    [Fact]
    public void FindChildren_WithStringKeyReturnsMatchedScalarKeyChildren()
    {
        var root = ReadYaml();

        var children = root.FindChildren<YamlScalarNode>("receipt");

        var child = Assert.Single(children);
        Assert.Equal("receipt", child.Key.Value);
        Assert.Equal("Oz-Ware Purchase Invoice", child.Value.Value);
        Assert.Equal(0, child.Index);
    }

    [Fact]
    public void FindChildren_WithYamlNodeKeyReturnsMatchedChildren()
    {
        var key = new YamlMappingNode { { "kind", "name" } };
        var value = new YamlScalarNode("Dorothy");
        var node = new YamlMappingNode
        {
            { key, value },
        };

        var children = node.FindChildren<YamlScalarNode>(key);

        var child = Assert.Single(children);
        Assert.Same(key, child.Key);
        Assert.Same(value, child.Value);
        Assert.Equal(0, child.Index);
    }

    [Fact]
    public void FindChildren_ReturnsEmptyListWhenNothingMatches()
    {
        var root = ReadYaml();

        var children = root.FindChildren<YamlScalarNode>("missing");

        Assert.Empty(children);
    }

    [Fact]
    public void HasChild_WithPredicateReturnsTrueWhenChildMatches()
    {
        var root = ReadYaml();

        var result = root.HasChild<YamlScalarNode>((k, v) => k.IsScalarWithValue("receipt") && v.Value == "Oz-Ware Purchase Invoice");

        Assert.True(result);
    }

    [Fact]
    public void HasChild_WithYamlNodeKeyReturnsTrueWhenTypedChildExists()
    {
        var key = new YamlMappingNode { { "kind", "name" } };
        var node = new YamlMappingNode
        {
            { key, new YamlScalarNode("Dorothy") },
        };

        var result = node.HasChild<YamlScalarNode>(key);

        Assert.True(result);
    }

    [Fact]
    public void HasChild_WithYamlNodeKeyReturnsFalseWhenValueTypeDoesNotMatch()
    {
        var key = new YamlScalarNode("name");
        var node = new YamlMappingNode
        {
            { key, new YamlScalarNode("Dorothy") },
        };

        var result = node.HasChild<YamlMappingNode>(key);

        Assert.False(result);
    }

    [Fact]
    public void HasChild_WithStringKeyReturnsTrueWhenTypedChildExists()
    {
        var root = ReadYaml();

        var result = root.HasChild<YamlMappingNode>("customer");

        Assert.True(result);
    }

    [Fact]
    public void HasChild_WithStringKeyAndValueReturnsTrueWhenScalarValueMatches()
    {
        var root = ReadYaml();

        var result = root.HasChild("receipt", "Oz-Ware Purchase Invoice");

        Assert.True(result);
    }

    [Fact]
    public void HasChild_WithStringKeyAndValueReturnsFalseWhenValueDoesNotMatch()
    {
        var root = ReadYaml();

        var result = root.HasChild("receipt", "wrong");

        Assert.False(result);
    }

    [Fact]
    public void MoveChildByKey_MovesChildAtEqualKeyNodeToDestinationIndex()
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

        var moved = node.MoveChildByKey(new YamlScalarNode("third"), 0);

        Assert.True(moved);
        Assert.Equal(new[] { "third", "first", "second" }, node.GetChildren().Select(m => m.Key.Value).ToArray());
    }

    [Fact]
    public void MoveChildByKey_ReturnsFalseWhenSourceAndDestinationAreSame()
    {
        var key = new YamlScalarNode("first");
        var node = new YamlMappingNode
        {
            { key, new YamlScalarNode("1") },
            { new YamlScalarNode("second"), new YamlScalarNode("2") },
        };

        var moved = node.MoveChildByKey(key, 0);

        Assert.False(moved);
        Assert.Equal(new[] { "first", "second" }, node.GetChildren().Select(m => m.Key.Value).ToArray());
    }

    [Fact]
    public void MoveChildByKeyReference_MovesChildAtKeyNodeReferenceToDestinationIndex()
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

        var moved = node.MoveChildByKeyReference(thirdKey, 0);

        Assert.True(moved);
        Assert.Equal(new[] { "third", "first", "second" }, node.GetChildren().Select(m => m.Key.Value).ToArray());
    }

    [Fact]
    public void MoveChildByKeyReference_ThrowsWhenKeyNodeReferenceDoesNotExist()
    {
        var node = new YamlMappingNode
        {
            { "first", "1" },
        };

        Assert.Throws<KeyNotFoundException>(() => node.MoveChildByKeyReference(new YamlScalarNode("first"), 0));
    }

    [Fact]
    public void MoveChildByValue_MovesChildAtEqualValueNodeToDestinationIndex()
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

        var moved = node.MoveChildByValue(new YamlScalarNode("3"), 0);

        Assert.True(moved);
        Assert.Equal(new[] { "third", "first", "second" }, node.GetChildren().Select(m => m.Key.Value).ToArray());
    }

    [Fact]
    public void MoveChildByValue_ReturnsFalseWhenSourceAndDestinationAreSame()
    {
        var value = new YamlScalarNode("1");
        var node = new YamlMappingNode
        {
            { new YamlScalarNode("first"), value },
            { new YamlScalarNode("second"), new YamlScalarNode("2") },
        };

        var moved = node.MoveChildByValue(value, 0);

        Assert.False(moved);
        Assert.Equal(new[] { "first", "second" }, node.GetChildren().Select(m => m.Key.Value).ToArray());
    }

    [Fact]
    public void MoveChildByValue_MovesFirstEqualValueNode()
    {
        var node = new YamlMappingNode
        {
            { new YamlScalarNode("first"), new YamlScalarNode("same") },
            { new YamlScalarNode("second"), new YamlScalarNode("same") },
            { new YamlScalarNode("third"), new YamlScalarNode("other") },
        };

        var moved = node.MoveChildByValue(new YamlScalarNode("same"), 2);

        Assert.True(moved);
        Assert.Equal(new[] { "second", "third", "first" }, node.GetChildren().Select(m => m.Key.Value).ToArray());
    }

    [Fact]
    public void MoveChildByValueReference_MovesChildAtValueNodeReferenceToDestinationIndex()
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

        var moved = node.MoveChildByValueReference(thirdValue, 0);

        Assert.True(moved);
        Assert.Equal(new[] { "third", "first", "second" }, node.GetChildren().Select(m => m.Key.Value).ToArray());
    }

    [Fact]
    public void MoveChildByValueReference_ThrowsWhenValueNodeReferenceDoesNotExist()
    {
        var node = new YamlMappingNode
        {
            { "first", "1" },
        };

        Assert.Throws<KeyNotFoundException>(() => node.MoveChildByValueReference(new YamlScalarNode("1"), 0));
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

    [Fact]
    public void MoveChildByKey_String_ReturnsFalseWhenSourceAndDestinationAreSame()
    {
        var node = new YamlMappingNode
        {
            { "first", "1" },
            { "second", "2" },
        };

        var moved = node.MoveChildByKey("first", 0);

        Assert.False(moved);
        Assert.Equal(new[] { "first", "second" }, node.GetChildren().Select(m => m.Key.Value).ToArray());
    }

    [Fact]
    public void MoveChildByKey_ThrowsWhenKeyDoesNotExist()
    {
        var node = new YamlMappingNode
        {
            { "first", "1" },
        };

        Assert.Throws<KeyNotFoundException>(() => node.MoveChildByKey("missing", 0));
    }

    [Fact]
    public void MoveChildByKey_ThrowsWhenDestinationIndexIsOutOfRange()
    {
        var node = new YamlMappingNode
        {
            { "first", "1" },
            { "second", "2" },
        };

        Assert.Throws<ArgumentOutOfRangeException>(() => node.MoveChildByKey("first", 2));
    }
}
