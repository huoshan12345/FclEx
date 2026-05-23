namespace AngleSharp.Dom;

public class HtmlAnchorTests
{
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