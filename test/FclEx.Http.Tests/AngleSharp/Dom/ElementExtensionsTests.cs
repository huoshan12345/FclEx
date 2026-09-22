namespace AngleSharp.Dom;

public class ElementExtensionsTests
{
    [Fact]
    public void AttributeShortcuts_ReturnAttributeValuesAndHandleNullElement()
    {
        var document = HtmlParser.Parse("""<html><body><input href="/next" type="text" value="alice" title="Name"></body></html>""");
        var element = document.QuerySelector("input");

        Assert.Equal("/next", element.Href());
        Assert.Equal("text", element.Type());
        Assert.Equal("alice", element.Value());
        Assert.Equal("Name", element.Title());

        IElement? nullElement = null;
        Assert.Null(nullElement.Href());
        Assert.Null(nullElement.Type());
        Assert.Null(nullElement.Value());
        Assert.Null(nullElement.Title());
    }

    [Theory]
    [InlineData("prefix", "value")]
    [InlineData("pre'fix", "value")]
    [InlineData("pre\\fix", "value")]
    [InlineData("pre]fix", "value")]
    public void QueryId_WhenPrefixContainsSelectorSyntax_EscapesPrefix(string prefix, string suffix)
    {
        var id = prefix + suffix;
        var html = $$"""
                     <html>
                     <body>
                         <div id="{{HtmlEncode(id)}}"></div>
                         <div id="other"></div>
                     </body>
                     </html>
                     """;
        var document = HtmlParser.Parse(html);

        var result = document.Body.QueryId(prefix);

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Equal(suffix, result.Value);
    }

    [Fact]
    public void QueryId_WhenPrefixContainsNewLine_EscapesPrefix()
    {
        const string prefix = "pre\nfix";
        const string suffix = "value";
        var html = $$"""
                     <html>
                     <body>
                         <div id="{{HtmlEncode(prefix + suffix)}}"></div>
                     </body>
                     </html>
                     """;
        var document = HtmlParser.Parse(html);

        var result = document.Body.QueryId(prefix);

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Equal(suffix, result.Value);
    }

    [Fact]
    public void QueryId_WhenNoElementMatches_ReturnsError()
    {
        var document = HtmlParser.Parse("<html><body><div id=\"other\"></div></body></html>");

        var result = document.Body.QueryId("missing'prefix");

        Assert.True(result.IsError);
    }

    [Fact]
    public void QueryHref_WhenHrefIsInvalid_ReturnsError()
    {
        var document = HtmlParser.Parse("""<html><body><a href="http://[">broken</a></body></html>""");

        var result = document.Body.QueryHref("a", new Uri("https://example.com"));

        Assert.True(result.IsError);
        Assert.IsAssignableFrom<UriFormatException>(result.Exception);
    }

    [Fact]
    public void QueryData_UsesFirstSelectorThatMatches()
    {
        var document = HtmlParser.Parse("""<html><body><span class="second">value</span></body></html>""");

        var result = document.Body.QueryData([".missing", ".second"], element => element.TextContent);

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Equal("value", result.Value.Data);
        Assert.Equal("second", result.Value.Element.ClassName);
    }

    [Fact]
    public void QueryData_WhenSelectorIsNull_UsesRootElement()
    {
        var document = HtmlParser.Parse("""<html><body><main>root</main></body></html>""");
        var root = document.QuerySelector("main");

        var result = root.QueryData(static element => element.TextContent);

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Same(root, result.Value.Element);
        Assert.Equal("root", result.Value.Data);
    }

    [Fact]
    public void QueryOwnText_ReturnsOnlyDirectTextAndTrimsByDefault()
    {
        var document = HtmlParser.Parse("""<html><body><div> hello <span>ignored</span> world </div></body></html>""");

        var result = document.Body.QueryOwnText("div");

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Equal("hello  world", result.Value.Text);
    }

    [Fact]
    public void QueryOwnText_WhenTextIsEmptyAndRequired_ReturnsError()
    {
        var document = HtmlParser.Parse("""<html><body><div><span>child text</span></div></body></html>""");

        var result = document.Body.QueryOwnText("div");

        Assert.True(result.IsError);
        Assert.Contains("own text is empty", result.Exception!.Message);
    }

    [Fact]
    public void QueryAttribute_WhenAttributeIsMissingOrEmpty_ReturnsErrorUnlessEmptyIsAllowed()
    {
        var document = HtmlParser.Parse("""<html><body><a href="">empty</a><span>none</span></body></html>""");

        var missing = document.Body.QueryAttribute("span", "href");
        var emptyRequired = document.Body.QueryAttribute("a", "href");
        var emptyAllowed = document.Body.QueryAttribute("a", "href", ensureValueIsNotEmpty: false);

        Assert.True(missing.IsError);
        Assert.Contains("No attribute 'href'", missing.Exception!.Message);
        Assert.True(emptyRequired.IsError);
        Assert.Contains("Attribute 'href' is empty", emptyRequired.Exception!.Message);
        Assert.True(emptyAllowed.IsSuccess, emptyAllowed.Exception?.ToString());
        Assert.Equal("", emptyAllowed.Value.Attribute);
    }

    [Fact]
    public void QueryHref_ResolvesRelativeHrefAgainstBaseUri()
    {
        var document = HtmlParser.Parse("""<html><body><a href="../next?q=1">next</a></body></html>""");

        var result = document.Body.QueryHref("a", new Uri("https://example.com/root/page"));

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Equal("https://example.com/next?q=1", result.Value.Href.Build().ToString());
    }

    private static string HtmlEncode(string value) => WebUtility.HtmlEncode(value);
}
