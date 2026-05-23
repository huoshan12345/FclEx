namespace System.Collections.Generic;

public class BiDictionaryTests
{
    [Fact]
    public void Add_ShouldAddKeyValuePairsAndAllowBidirectionalLookup()
    {
        var biDict = new BiDictionary<int, string>
        {
            { 1, "One" },
            { 2, "Two" },
        };

        Assert.Equal("One", biDict[1]);
        Assert.Equal(1, biDict["One"]);
        Assert.Equal("Two", biDict[2]);
        Assert.Equal(2, biDict["Two"]);
    }

    [Fact]
    public void RemoveByKey_ShouldRemoveKeyAndValue()
    {
        var biDict = new BiDictionary<int, string> { { 1, "One" } };

        biDict.Remove(1);

        Assert.False(biDict.ContainsKey(1));
        Assert.False(biDict.ContainsValue("One"));
    }

    [Fact]
    public void RemoveByValue_ShouldRemoveKeyAndValue()
    {
        var biDict = new BiDictionary<int, string> { { 1, "One" } };

        biDict.Remove("One");

        Assert.False(biDict.ContainsKey(1));
        Assert.False(biDict.ContainsValue("One"));
    }

    [Fact]
    public void IndexerSet_ShouldUpdateKeyValuePair()
    {
        var biDict = new BiDictionary<int, string> { { 1, "One" } };

        biDict[1] = "Updated";

        Assert.Equal("Updated", biDict[1]);
        Assert.Equal(1, biDict["Updated"]);
        Assert.False(biDict.ContainsValue("One"));
    }

    [Fact]
    public void ContainsKey_ShouldReturnTrueIfKeyExists()
    {
        var biDict = new BiDictionary<int, string> { { 1, "One" } };

        Assert.True(biDict.ContainsKey(1));
        Assert.False(biDict.ContainsKey(2));
    }

    [Fact]
    public void ContainsValue_ShouldReturnTrueIfValueExists()
    {
        var biDict = new BiDictionary<int, string> { { 1, "One" } };

        Assert.True(biDict.ContainsValue("One"));
        Assert.False(biDict.ContainsValue("Two"));
    }

    [Fact]
    public void Clear_ShouldRemoveAllItems()
    {
        var biDict = new BiDictionary<int, string>
        {
            { 1, "One" },
            { 2, "Two" },
        };

        biDict.Clear();

        Assert.Empty(biDict);
    }

    [Fact]
    public void TryGetValue_ShouldReturnValueIfKeyExists()
    {
        var biDict = new BiDictionary<int, string> { { 1, "One" } };

        Assert.True(biDict.TryGetValue(1, out var value));
        Assert.Equal("One", value);
    }

    [Fact]
    public void TryGetKey_ShouldReturnKeyIfValueExists()
    {
        var biDict = new BiDictionary<int, string> { { 1, "One" } };

        Assert.True(biDict.TryGetKey("One", out var key));
        Assert.Equal(1, key);
    }

    [Fact]
    public void DuplicateKey_ShouldThrowException()
    {
        var biDict = new BiDictionary<int, string> { { 1, "One" } };

        Assert.Throws<ArgumentException>(() => biDict.Add(1, "Duplicate"));
    }

    [Fact]
    public void DuplicateValue_ShouldThrowException()
    {
        var biDict = new BiDictionary<int, string> { { 1, "One" } };

        Assert.Throws<ArgumentException>(() => biDict.Add(2, "One"));
    }
}