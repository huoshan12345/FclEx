namespace FclEx.Utils.Collections;

public class NameValuesTests
{
    [Fact]
    public void Constructor_InitializesEmptyCollection()
    {
        // Arrange & Act
        var nameValues = new NameValues(StringComparer.OrdinalIgnoreCase);

        // Assert
        Assert.Equal(0, nameValues.Count);
        Assert.Empty(nameValues);
    }

    [Fact]
    public void Add_IncreasesCount()
    {
        // Arrange
        var nameValues = new NameValues(StringComparer.OrdinalIgnoreCase);

        // Act
        nameValues.Add("key", "value");

        // Assert
        Assert.Equal(1, nameValues.Count);
    }

    [Fact]
    public void Add_WithNullKeyAndValue_AddsEmptyStrings()
    {
        // Arrange
        var nameValues = new NameValues(StringComparer.OrdinalIgnoreCase);

        // Act
        nameValues.Add(null, null);

        // Assert
        Assert.Equal(1, nameValues.Count);
        Assert.Equal("", nameValues.Get(""));
    }

    [Fact]
    public void Add_WithSameKey_AllowsMultipleValues()
    {
        // Arrange
        var nameValues = new NameValues(StringComparer.OrdinalIgnoreCase);

        // Act
        nameValues.Add("color", "red")
            .Add("color", "blue")
            .Add("color", "green");

        // Assert
        Assert.Equal(3, nameValues.Count);
        Assert.Equal("green", nameValues.Get("color"));
            
        var values = nameValues.GetValues("color");
        Assert.NotNull(values);
        Assert.Equal(3, values.Count);
        Assert.Contains("red", values);
        Assert.Contains("blue", values);
        Assert.Contains("green", values);
    }

    [Fact]
    public void Set_ReplacesExistingValues()
    {
        // Arrange
        var nameValues = new NameValues(StringComparer.OrdinalIgnoreCase);
        nameValues.Add("color", "red")
            .Add("color", "blue");

        // Act
        nameValues.Set("color", "green");

        // Assert
        Assert.Equal(1, nameValues.Count);
        Assert.Equal("green", nameValues.Get("color"));
            
        var values = nameValues.GetValues("color");
        Assert.NotNull(values);
        Assert.Single(values);
        Assert.Contains("green", values);
    }

    [Fact]
    public void Set_MultipleValues_ReplacesExistingValuesAndPreservesAllNewValues()
    {
        var nameValues = new NameValues(StringComparer.OrdinalIgnoreCase)
            .Add("color", "red")
            .Add("color", "blue");
        KeyValuePair<string, string[]>[] pairs =
        [
            new("color", ["green", "yellow"]),
        ];

        nameValues.Set(pairs);

        Assert.Equal(["green", "yellow"], nameValues.GetValues("color"));
    }

    [Fact]
    public void Set_EmptyValueSequence_RemovesExistingKey()
    {
        var nameValues = new NameValues(StringComparer.OrdinalIgnoreCase)
            .Add("color", "red");
        KeyValuePair<string, string[]>[] pairs =
        [
            new("color", []),
        ];

        nameValues.Set(pairs);

        Assert.False(nameValues.ContainsKey("color"));
    }

    [Fact]
    public void Get_ReturnsLastValue()
    {
        // Arrange
        var nameValues = new NameValues(StringComparer.OrdinalIgnoreCase);
        nameValues.Add("color", "red")
            .Add("color", "blue")
            .Add("color", "green");

        // Act
        var result = nameValues.Get("color");

        // Assert
        Assert.Equal("green", result);
    }

    [Fact]
    public void Get_WithNonExistentKey_ReturnsNull()
    {
        // Arrange
        var nameValues = new NameValues(StringComparer.OrdinalIgnoreCase);
        nameValues.Add("color", "red");

        // Act
        var result = nameValues.Get("size");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetValues_ReturnsAllValues()
    {
        // Arrange
        var nameValues = new NameValues(StringComparer.OrdinalIgnoreCase);
        nameValues.Add("color", "red")
            .Add("color", "blue")
            .Add("color", "green");

        // Act
        var values = nameValues.GetValues("color");

        // Assert
        Assert.NotNull(values);
        Assert.Equal(3, values.Count);
        Assert.Equal(new[] { "red", "blue", "green" }, values);
    }

    [Fact]
    public void GetValues_WithNonExistentKey_ReturnsNull()
    {
        // Arrange
        var nameValues = new NameValues(StringComparer.OrdinalIgnoreCase);
        nameValues.Add("color", "red");

        // Act
        var values = nameValues.GetValues("size");

        // Assert
        Assert.Null(values);
    }

    [Fact]
    public void TryGet_WithExistingKey_ReturnsLastValue()
    {
        // Arrange
        var nameValues = new NameValues(StringComparer.OrdinalIgnoreCase);
        nameValues.Add("color", "red")
            .Add("color", "blue");

        // Act
        var success = nameValues.TryGet("color", out var value);

        // Assert
        Assert.True(success);
        Assert.Equal("blue", value);
    }

    [Fact]
    public void TryGet_WithNonExistentKey_ReturnsFalse()
    {
        // Arrange
        var nameValues = new NameValues(StringComparer.OrdinalIgnoreCase);
        nameValues.Add("color", "red");

        // Act
        var success = nameValues.TryGet("size", out var value);

        // Assert
        Assert.False(success);
        Assert.Null(value);
    }

    [Fact]
    public void TryGetValues_WithExistingKey_ReturnsAllValues()
    {
        // Arrange
        var nameValues = new NameValues(StringComparer.OrdinalIgnoreCase);
        nameValues.Add("color", "red")
            .Add("color", "blue");

        // Act
        var success = nameValues.TryGetValues("color", out var values);

        // Assert
        Assert.True(success);
        Assert.NotNull(values);
        Assert.Equal(2, values.Count);
        Assert.Equal(new[] { "red", "blue" }, values);
    }

    [Fact]
    public void TryGetValues_WithNonExistentKey_ReturnsFalse()
    {
        // Arrange
        var nameValues = new NameValues(StringComparer.OrdinalIgnoreCase);
        nameValues.Add("color", "red");

        // Act
        var success = nameValues.TryGetValues("size", out var values);

        // Assert
        Assert.False(success);
        Assert.Null(values);
    }

    [Fact]
    public void Remove_ExistingKey_RemovesAllValues()
    {
        // Arrange
        var nameValues = new NameValues(StringComparer.OrdinalIgnoreCase);
        nameValues.Add("color", "red")
            .Add("color", "blue")
            .Add("size", "large");

        // Act
        nameValues.Remove("color");

        // Assert
        Assert.Equal(1, nameValues.Count);
        Assert.Null(nameValues.Get("color"));
        Assert.Equal("large", nameValues.Get("size"));
    }

    [Fact]
    public void Remove_NonExistentKey_DoesNothing()
    {
        // Arrange
        var nameValues = new NameValues(StringComparer.OrdinalIgnoreCase);
        nameValues.Add("color", "red");
        var initialCount = nameValues.Count;

        // Act
        nameValues.Remove("size");

        // Assert
        Assert.Equal(initialCount, nameValues.Count);
    }

    [Fact]
    public void Clear_RemovesAllEntries()
    {
        // Arrange
        var nameValues = new NameValues(StringComparer.OrdinalIgnoreCase);
        nameValues.Add("color", "red")
            .Add("size", "large");

        // Act
        nameValues.Clear();

        // Assert
        Assert.Equal(0, nameValues.Count);
        Assert.Empty(nameValues);
    }

    [Fact]
    public void Indexer_Get_ReturnsLastValue()
    {
        // Arrange
        var nameValues = new NameValues(StringComparer.OrdinalIgnoreCase);
        nameValues.Add("color", "red")
            .Add("color", "blue");

        // Act
        var value = nameValues["color"];

        // Assert
        Assert.Equal("blue", value);
    }

    [Fact]
    public void Indexer_Set_ReplacesExistingValues()
    {
        // Arrange
        var nameValues = new NameValues(StringComparer.OrdinalIgnoreCase);
        nameValues.Add("color", "red")
            .Add("color", "blue");

        // Act
        nameValues["color"] = "green";

        // Assert
        Assert.Equal("green", nameValues.Get("color"));
        Assert.Equal(1, nameValues.Count);
    }

    [Fact]
    public void Enumerator_YieldsAllKeyValuePairs()
    {
        // Arrange
        var nameValues = new NameValues(StringComparer.OrdinalIgnoreCase);
        nameValues.Add("color", "red")
            .Add("color", "blue")
            .Add("size", "large");

        // Act
        var pairs = nameValues.ToList();

        // Assert
        Assert.Equal(3, pairs.Count);
        Assert.Contains(pairs, pair => pair.Key == "color" && pair.Value == "red");
        Assert.Contains(pairs, pair => pair.Key == "color" && pair.Value == "blue");
        Assert.Contains(pairs, pair => pair.Key == "size" && pair.Value == "large");
    }

    [Fact]
    public void Enumerator_ThrowsException_WhenCollectionModifiedDuringEnumeration()
    {
        // Arrange
        var nameValues = new NameValues(StringComparer.OrdinalIgnoreCase);
        nameValues.Add("color", "red")
            .Add("size", "large");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            foreach (var pair in nameValues)
            {
                nameValues.Add("weight", "heavy");
            }
        });
    }

    [Fact]
    public void StringComparer_AffectsKeyComparison()
    {
        // Arrange
        var caseInsensitiveNameValues = new NameValues(StringComparer.OrdinalIgnoreCase);
        var caseSensitiveNameValues = new NameValues(StringComparer.Ordinal);

        // Act
        caseInsensitiveNameValues.Add("Color", "red");
        caseInsensitiveNameValues.Add("color", "blue");

        caseSensitiveNameValues.Add("Color", "red");
        caseSensitiveNameValues.Add("color", "blue");

        // Assert
        // Case insensitive should treat the keys as the same
        Assert.Equal(2, caseInsensitiveNameValues.Count);
        Assert.Equal("blue", caseInsensitiveNameValues.Get("COLOR"));
        Assert.Equal(2, caseInsensitiveNameValues.GetValues("color")?.Count);

        // Case sensitive should treat the keys as different
        Assert.Equal(2, caseSensitiveNameValues.Count);
        Assert.Equal("red", caseSensitiveNameValues.Get("Color"));
        Assert.Equal("blue", caseSensitiveNameValues.Get("color"));
        Assert.Equal(1, caseSensitiveNameValues.GetValues("Color")?.Count);
        Assert.Equal(1, caseSensitiveNameValues.GetValues("color")?.Count);
    }

    [Fact]
    public void NonGenericEnumerator_ReturnsAllPairs()
    {
        // Arrange
        var nameValues = new NameValues(StringComparer.OrdinalIgnoreCase);
        nameValues.Add("color", "red")
            .Add("size", "large");

        // Act
        var count = 0;
        var enumerator = ((IEnumerable)nameValues).GetEnumerator();
            
        while (enumerator.MoveNext())
        {
            count++;
            var pair = (KeyValuePair<string, string>)enumerator.Current;
            Assert.True(pair.Key == "color" || pair.Key == "size");
            Assert.True(pair.Value == "red" || pair.Value == "large");
        }

        // Assert
        Assert.Equal(2, count);
    }
}
