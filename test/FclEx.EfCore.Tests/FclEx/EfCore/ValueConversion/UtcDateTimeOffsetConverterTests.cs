namespace FclEx.EfCore.ValueConversion;

public class UtcDateTimeOffsetConverterTests
{
    [Theory]
    [InlineData(-5)]
    [InlineData(0)]
    [InlineData(8)]
    public void Convert_NormalizesWritesToUtc_AndLeavesReadsUnchanged(int offsetHours)
    {
        var converter = new UtcDateTimeOffsetConverter();
        var value = new DateTimeOffset(2026, 10, 23, 12, 0, 0, TimeSpan.FromHours(offsetHours));

        var written = Assert.IsType<DateTimeOffset>(converter.ConvertToProvider(value));
        Assert.Equal(TimeSpan.Zero, written.Offset);
        Assert.Equal(value.UtcDateTime, written.UtcDateTime);
        var read = Assert.IsType<DateTimeOffset>(converter.ConvertFromProvider(value));
        Assert.Equal(value.Ticks, read.Ticks);
        Assert.Equal(value.Offset, read.Offset);
        Assert.Null(converter.ConvertToProvider(null));
        Assert.Null(converter.ConvertFromProvider(null));
    }
}
