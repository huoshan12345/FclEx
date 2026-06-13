using AngleSharp.Html.Dom;

namespace AngleSharp.Dom;

public class HtmlAnchorTests
{
    [Fact]
    public void Constructor_ParsesPathQueryTextAndTitle()
    {
        var document = HtmlHelper.Parse("""<html><body><a href="/items?id=42&tag=a" title="Details">Open</a></body></html>""");
        var element = document.QuerySelector<IHtmlAnchorElement>("a");
        Assert.NotNull(element);

        var anchor = new HtmlAnchor(element);

        var (text, path, query, title, deconstructedElement) = anchor;
        Assert.Equal("Open", text);
        Assert.EndsWith("/items", path);
        Assert.Equal("42", query["id"]);
        Assert.Equal("a", query["tag"]);
        Assert.Equal("Details", title);
        Assert.Same(element, deconstructedElement);
    }

    [Fact]
    public void Empty_test()
    {
        var (text, path, query, title, element) = HtmlAnchor.Empty;
        Assert.Equal(string.Empty, text);
        Assert.Equal(string.Empty, path);
        Assert.Empty(query);
        Assert.Equal(string.Empty, title);
        Assert.NotNull(element);
    }
}
