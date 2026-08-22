namespace AngleSharp.Dom;

public class ParentNodeExtensionsTests
{
    [Fact]
    public void RemoveJsCss_RemovesScriptAndStyleDescendantsAndReturnsSameNode()
    {
        var document = HtmlParser.Parse("""
                                        <html>
                                        <head>
                                            <style>.hidden { display: none; }</style>
                                            <script>window.test = true;</script>
                                        </head>
                                        <body>
                                            <main><p>content</p></main>
                                        </body>
                                        </html>
                                        """);

        var returned = document.RemoveJsCss();

        Assert.Same(document, returned);
        Assert.Empty(document.QuerySelectorAll("script, style"));
        Assert.NotNull(document.QuerySelector("main p"));
    }

    [Fact]
    public void RemoveJsCss_WhenNodeIsNull_ReturnsNull()
    {
        IParentNode? node = null;

        var result = node.RemoveJsCss();

        Assert.Null(result);
    }
}
