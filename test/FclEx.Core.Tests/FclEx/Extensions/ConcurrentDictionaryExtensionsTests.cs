namespace FclEx.Extensions;

public class ConcurrentDictionaryExtensionsTests
{
    [Fact]
    public void Remove_ReturnsWhetherTheKeyWasRemoved()
    {
        var dictionary = new ConcurrentDictionary<string, int>(new[] { new KeyValuePair<string, int>("one", 1) });

        Assert.True(dictionary.Remove("one"));
        Assert.False(dictionary.Remove("one"));
    }
}
