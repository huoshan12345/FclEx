namespace System.Collections.Generic;

public class MultiValueDictionaryTests
{
    [Fact]
    public void AddRange_ShouldNotAddKeyForEmptyValues()
    {
        var dictionary = new MultiValueDictionary<string, int>();

        dictionary.AddRange("key", []);

        Assert.False(dictionary.ContainsKey("key"));
        Assert.Empty(dictionary);
    }

    [Fact]
    public void AddRange_ShouldNotAddNewKeyWhenEnumerationFails()
    {
        var dictionary = new MultiValueDictionary<string, int>();

        Assert.Throws<InvalidOperationException>(() => dictionary.AddRange("key", ValuesThatThrow()));

        Assert.False(dictionary.ContainsKey("key"));
        Assert.Empty(dictionary);
    }

    private static IEnumerable<int> ValuesThatThrow()
    {
        yield return 1;
        throw new InvalidOperationException("Enumeration failed.");
    }
}
