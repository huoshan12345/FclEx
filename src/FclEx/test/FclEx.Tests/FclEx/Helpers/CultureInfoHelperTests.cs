using System.Globalization;

namespace FclEx.Helpers;

public class CultureInfoHelperTests
{
    private readonly ITestOutputHelper _output;

    public CultureInfoHelperTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void TestParse()
    {
        var str = "Thu, 31-Dec-37 23:55:55 GMT";
        var format = "ddd, d-MMM-yy HH:mm:ss Z";
        var parsed = (DateTime.TryParseExact(str, format, CultureInfoHelper.TwoDigitYear,
            DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal,
            out var time));
        Assert.True(parsed);
        _output.WriteLine(time.ToString("yyyy-MM-dd HH:mm:ss"));
    }
}