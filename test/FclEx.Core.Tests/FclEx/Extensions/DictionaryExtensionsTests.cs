namespace FclEx.Extensions;

public class DictionaryExtensionsTests
{
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
