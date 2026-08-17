namespace FclEx.Utils;

public class INameValuesBuilderTests
{
    private sealed class FormattedBuilder : NameValuesBuilder
    {
        [NameValue("date", Format = "yyyy-MM-dd")]
        public DateTime Date { get; } = new(2026, 8, 16);
    }

    [Fact]
    public void Build_UsesTheFormatSpecifiedByNameValueAttribute()
    {
        var value = new FormattedBuilder().Build().Single();

        Assert.Equal(new KeyValuePair<string, string>("date", "2026-08-16"), value);
    }
}
