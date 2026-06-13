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
    [InlineData("""<meta charset='gb2312;' >""", "gb2312")]
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

    [Fact]
    public void Parse_ReturnsHtmlDocument()
    {
        var document = HtmlHelper.Parse("<html><body><h1>Hello</h1></body></html>");

        Assert.Equal("Hello", document.QuerySelector("h1")?.TextContent);
    }

    [Fact]
    public async Task ParseAsync_ReturnsHtmlDocument()
    {
        var document = await HtmlHelper.ParseAsync("<html><body><h1>Hello</h1></body></html>");

        Assert.Equal("Hello", document.QuerySelector("h1")?.TextContent);
    }

    [Fact]
    public void GetTextContent_ReturnsBodyText()
    {
        var text = HtmlHelper.GetTextContent("<html><head><title>Title</title></head><body><p>Hello <b>world</b></p></body></html>");

        Assert.Equal("Hello world", text);
    }

    [Fact]
    public void RemoveHtmlTags_WhenBodyExists_ReturnsBodyText()
    {
        var text = HtmlHelper.RemoveHtmlTags("<p>Hello <b>world</b></p>");

        Assert.Equal("Hello world", text);
    }

    [Fact]
    public void RemoveHtmlTags_WhenDocumentHasNoBodyText_ReturnsEmptyString()
    {
        var text = HtmlHelper.RemoveHtmlTags("<!doctype html><html><head><title>Only title</title></head></html>");

        Assert.Equal("", text);
    }
}
