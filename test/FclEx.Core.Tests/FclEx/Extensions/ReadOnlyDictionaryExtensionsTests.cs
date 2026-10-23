namespace FclEx.Extensions;

public class ReadOnlyDictionaryExtensionsTests
{
    [Theory]
    [InlineData("2026/10/23 12:34 +02:00", "yyyy/MM/dd HH:mm zzz", true)]
    [InlineData("2026-10-23T12:34:56+00:00", "yyyy/MM/dd HH:mm zzz", false)]
    [InlineData("invalid", null, false)]
    [InlineData(null, null, false)]
    [InlineData("", null, false)]
    public void TryGetDateTimeOffset_RespectsExactFormat_AndResetsValueOnFailure(
        string? text, string? format, bool expected)
    {
        IReadOnlyDictionary<string, string?> dictionary = new Dictionary<string, string?> { ["date"] = text };
        Assert.Equal(expected, dictionary.TryGetDateTimeOffset("date", out var value,
            format, CultureInfo.InvariantCulture));
        if (expected)
            Assert.Equal(new DateTimeOffset(2026, 10, 23, 12, 34, 0, TimeSpan.FromHours(2)), value);
        else
            Assert.Equal(default, value);

        Assert.False(dictionary.TryGetDateTimeOffset("missing", out value));
        Assert.Equal(default, value);
    }

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
