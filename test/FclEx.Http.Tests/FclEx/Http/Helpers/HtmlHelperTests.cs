namespace FclEx.Http.Helpers;

public class HtmlHelperTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void GetMetaCharSet_WhenHtmlIsNullOrEmpty_ReturnsNull(string? html)
    {
        Assert.Null(HtmlHelper.GetMetaCharSet(html!));
    }

    [Theory]
    [InlineData("""<meta charset="utf-8;">""", "utf-8")]
    [InlineData("<meta charset='gb2312;' >", "gb2312")]
    [InlineData("""<meta http-equiv="Content-Type" content="text/html; charset=utf-8;">""", "utf-8")]
    [InlineData("""<meta content="text/html; charset='Shift_JIS'">""", "Shift_JIS")]
    public void GetMetaCharSet_TrimsQuotesSpacesAndTrailingSemicolon(string html, string expected)
    {
        Assert.Equal(expected, HtmlHelper.GetMetaCharSet(html));
    }

    [Fact]
    public void GetMetaCharSet_WhenCharsetMetaIsMissing_ReturnsNull()
    {
        var charset = HtmlHelper.GetMetaCharSet("""<meta name="viewport" content="width=device-width">""");

        Assert.Null(charset);
    }
}
