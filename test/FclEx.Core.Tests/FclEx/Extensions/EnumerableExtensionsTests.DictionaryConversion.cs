namespace FclEx.Extensions;

public class EnumerableDictionaryConversionTests
{
    [Theory]
    [InlineData("AsDictionary")]
    [InlineData("AsIDictionary")]
    [InlineData("AsIReadOnlyDictionary")]
    public void Conversion_WithExplicitComparer_UsesRequestedKeyEquality(string conversion)
    {
        IEnumerable<KeyValuePair<string, int>> source = new Dictionary<string, int>
        {
            ["Key"] = 1,
        };

        var found = conversion switch
        {
            "AsDictionary" => source.AsDictionary(StringComparer.OrdinalIgnoreCase).ContainsKey("KEY"),
            "AsIDictionary" => source.AsIDictionary(StringComparer.OrdinalIgnoreCase).ContainsKey("KEY"),
            "AsIReadOnlyDictionary" => source.AsIReadOnlyDictionary(StringComparer.OrdinalIgnoreCase).ContainsKey("KEY"),
            _ => throw new ArgumentOutOfRangeException(nameof(conversion)),
        };

        Assert.True(found);
    }
}
