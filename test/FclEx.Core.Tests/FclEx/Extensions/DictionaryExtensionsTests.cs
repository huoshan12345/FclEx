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
}
