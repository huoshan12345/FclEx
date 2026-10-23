namespace FclEx.Extensions;

public class ReadOnlyDictionaryExtensionsTests
{
    [Fact]
    public void TryGetDateTimeOffset_WithoutFormat_ParsesIsoTimestamp()
    {
        IReadOnlyDictionary<string, string> dictionary = new Dictionary<string, string>
        {
            ["timestamp"] = "2026-10-23T12:34:56+00:00",
        };

        var success = dictionary.TryGetDateTimeOffset("timestamp", out var value,
            formatProvider: CultureInfo.InvariantCulture);

        Assert.True(success);
        Assert.Equal(new DateTimeOffset(2026, 10, 23, 12, 34, 56, TimeSpan.Zero), value);
    }
}
