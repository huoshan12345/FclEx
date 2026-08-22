namespace AngleSharp.Html.Parser;

public class HtmlParserExtensionsTests
{
    [Fact]
    public void Parse_ReturnsHtmlDocument()
    {
        var document = HtmlParser.Parse("<html><body><h1>Hello</h1></body></html>");

        Assert.Equal("Hello", document.QuerySelector("h1")?.TextContent);
    }

    [Fact]
    public async Task ParseAsync_ReturnsHtmlDocument()
    {
        var document = await HtmlParser.ParseAsync("<html><body><h1>Hello</h1></body></html>");

        Assert.Equal("Hello", document.QuerySelector("h1")?.TextContent);
    }
}
