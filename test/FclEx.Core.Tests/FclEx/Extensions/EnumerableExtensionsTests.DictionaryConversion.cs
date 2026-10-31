namespace FclEx.Extensions;

public class EnumerableDictionaryConversionTests
{
    [Fact]
    public void AsDictionary_ReusesOnlyCompatibleComparer_AndRejectsNewDuplicateKeys()
    {
        var source = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) { ["Key"] = 1 };
        Assert.Same(source, source.AsDictionary(StringComparer.OrdinalIgnoreCase));

        var defaultEquality = source.AsDictionary();
        Assert.NotSame(source, defaultEquality);
        Assert.False(defaultEquality.ContainsKey("KEY"));
        var ordinary = new Dictionary<string, int> { ["key"] = 1 };
        Assert.Same(ordinary, ordinary.AsDictionary());

        var duplicates = new Dictionary<string, int> { ["key"] = 1, ["KEY"] = 2 };
        Assert.Throws<ArgumentException>(() => duplicates.AsDictionary(StringComparer.OrdinalIgnoreCase));
        Assert.Throws<ArgumentNullException>(() => ((IEnumerable<KeyValuePair<string, int>>)null!).AsDictionary());
    }

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
