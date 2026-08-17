namespace FclEx.Extensions;

public class DateTimeOffsetExtensionsTests
{
    [Fact]
    public void ToChinaStandardTime_Preserves_The_Instant_And_Uses_UtcPlusEight()
    {
        var instant = new DateTimeOffset(2024, 2, 14, 12, 34, 56, TimeSpan.FromHours(-3.5));

        var result = instant.ToChinaStandardTime();

        Assert.Equal(TimeSpan.FromHours(8), result.Offset);
        Assert.Equal(instant.UtcDateTime, result.UtcDateTime);
    }
}
