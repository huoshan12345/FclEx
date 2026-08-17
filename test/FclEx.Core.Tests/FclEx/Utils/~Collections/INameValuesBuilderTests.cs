namespace FclEx.Utils;

public class INameValuesBuilderTests
{
    private sealed class FormattedBuilder : NameValuesBuilder
    {
        [NameValue("date", Format = "yyyy-MM-dd")]
        public DateTime Date { get; } = new(2026, 8, 16);
    }

    private sealed class DefaultValueBuilder : NameValuesBuilder
    {
        [NameValue("count", OmitOption = NameValueOmitOption.WhenDefault)]
        public int Count { get; }
    }

    private sealed class NeverOmitBuilder : NameValuesBuilder
    {
        [NameValue("value", OmitOption = NameValueOmitOption.Never | NameValueOmitOption.WhenNull)]
        public string? Value { get; }
    }

    [Fact]
    public void Build_UsesTheFormatSpecifiedByNameValueAttribute()
    {
        var value = new FormattedBuilder().Build().Single();
        Assert.Equal(new KeyValuePair<string, string>("date", "2026-08-16"), value);
    }

    [Fact]
    public void Build_OmitsDefaultValueTypesWhenRequested()
    {
        Assert.Empty(new DefaultValueBuilder().Build());
    }

    [Fact]
    public void Build_NeverTakesPrecedenceOverOtherOmitFlags()
    {
        Assert.Equal([new KeyValuePair<string, string>("value", "")], new NeverOmitBuilder().Build());
    }
}
