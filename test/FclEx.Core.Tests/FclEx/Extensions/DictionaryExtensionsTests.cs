namespace FclEx.Extensions;

public class DictionaryExtensionsTests
{
    [Fact]
    public void AsReadOnlyDictionary_WrapsAMutableDictionary()
    {
        IDictionary<string, int> dictionary = new Dictionary<string, int> { ["one"] = 1 };

        var readOnly = dictionary.AsReadOnly();

        Assert.NotSame(dictionary, readOnly);
        Assert.Throws<NotSupportedException>(() => ((IDictionary<string, int>)readOnly).Add("two", 2));
        dictionary["one"] = 10;
        Assert.Equal(10, readOnly["one"]);
    }

    [Fact]
    public void Get_WhenKeyExistsWithNullValue_DoesNotUseFallback()
    {
        IDictionary<string, string?> dictionary = new Dictionary<string, string?>
        {
            ["present"] = null
        };

        var value = dictionary.Get("present", "fallback");

        Assert.Null(value);
    }

    [Fact]
    public void Add_CreatesAndReusesTheCollectionForAKey()
    {
        IDictionary<string, List<int>> dictionary = new Dictionary<string, List<int>>();

        dictionary.Add("numbers", 1);
        dictionary.Add("numbers", 2);

        Assert.Equal([1, 2], dictionary["numbers"]);
    }

    [Fact]
    public void TrySet_WhenOverwriteIsFalse_AddsOnlyMissingKeys()
    {
        IDictionary<string, int> dictionary = new Dictionary<string, int> { ["existing"] = 1 };

        Assert.True(dictionary.TrySet("new", 2, overwrite: false));
        Assert.False(dictionary.TrySet("existing", 3, overwrite: false));

        Assert.Equal(2, dictionary["new"]);
        Assert.Equal(1, dictionary["existing"]);
    }

    [Fact]
    public void TrySet_WhenOverwriteIsTrue_AddsOrReplacesTheValue()
    {
        IDictionary<string, int> dictionary = new Dictionary<string, int> { ["existing"] = 1 };

        Assert.True(dictionary.TrySet("new", 2, overwrite: true));
        Assert.True(dictionary.TrySet("existing", 3, overwrite: true));

        Assert.Equal(2, dictionary["new"]);
        Assert.Equal(3, dictionary["existing"]);
    }

    [Fact]
    public void TrySet_WhenDictionaryIsNull_ThrowsArgumentNullException()
    {
        IDictionary<string, int>? dictionary = null;

        Assert.Throws<ArgumentNullException>(() => dictionary!.TrySet("key", 1, overwrite: true));
    }
}
