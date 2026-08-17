using System.Collections.ObjectModel;

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

    [Fact]
    public void Add_ShouldNotAddNewKeyWhenValueCollectionRejectsValue()
    {
        var dictionary = MultiValueDictionary<string, int>.Create(() => new ThrowingCollection());

        Assert.Throws<InvalidOperationException>(() => dictionary.Add("key", 1));

        Assert.False(dictionary.ContainsKey("key"));
        Assert.Empty(dictionary);
    }

    private static IEnumerable<int> ValuesThatThrow()
    {
        yield return 1;
        throw new InvalidOperationException("Enumeration failed.");
    }

    private sealed class ThrowingCollection : Collection<int>
    {
        protected override void InsertItem(int index, int item)
        {
            throw new InvalidOperationException("The value was rejected.");
        }
    }
}
