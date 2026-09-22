namespace FclEx.Http.Actions;

public class HtmlActionContextTests
{
    [Theory]
    [InlineData(null, "HTML", 1)]
    [InlineData("html", "HTML", 1)]
    [InlineData(":root", "HTML", 1)]
    [InlineData("li", "LI", 2)]
    [InlineData(".missing", null, 0)]
    public void Constructor_ExposesDocumentAndSelectedElements(string? selector, string? tagName, int count)
    {
        const string html = "<html><head><title>Title</title></head><body><ul><li>A</li><li>B</li></ul></body></html>";
        var response = HttpActionTestFixtures.CreateResponse(html);
        using var context = new HtmlActionContext(response, html, selector);

        Assert.Same(response, context.Response);
        Assert.Equal(html, context.Html);
        Assert.Equal(selector, context.HtmlSelector);
        Assert.Equal("Title", context.Document.Title);
        Assert.Equal(count, context.ResultElements.Length);
        Assert.Equal(tagName, context.ResultElement?.TagName);
#pragma warning disable CS0618
        Assert.Same(context.Document.DocumentElement, context.Element);
#pragma warning restore CS0618
        Assert.All(context.ResultElements, element => Assert.Same(context.Document, element.Owner));
    }

    [Fact]
    public void Constructor_WhenSelectorIsInvalid_ThrowsDomException()
    {
        var response = HttpActionTestFixtures.CreateResponse();

        Assert.Throws<AngleSharp.Dom.DomException>(() => new HtmlActionContext(response, "<p>content</p>", "["));
    }

    [Fact]
    public void Dispose_ReleasesDocument()
    {
        var response = HttpActionTestFixtures.CreateResponse("<p>content</p>");
        var context = new HtmlActionContext(response, response.ResponseString!, null);
        var document = context.Document;
        Assert.NotNull(document.DocumentElement);

        context.Dispose();

        // AngleSharp removes the document's children when it is disposed.
        Assert.Null(document.DocumentElement);
        Assert.Equal("<p>content</p>", response.ResponseString);
    }
}
