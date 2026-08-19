namespace FclEx.Extensions;

public class CultureInfoExtensionsTests
{
    [Fact]
    public void DateTime_TryParseExact_Test()
    {
        const string str = "Thu, 31-Dec-37 23:55:55 GMT";
        const string format = "ddd, d-MMM-yy HH:mm:ss Z";
        var parsed = (DateTime.TryParseExact(str, format, CultureInfo.TwoDigitYear,
            DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal,
            out var time));
        Assert.True(parsed);
        Assert.Equal(new DateTime(2037, 12, 31, 23, 55, 55, DateTimeKind.Utc), time.ToUniversalTime());
    }
}