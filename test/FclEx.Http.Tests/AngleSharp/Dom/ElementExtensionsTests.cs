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

    [Fact]
    public void GetAnchor_WhenElementOrSelectedChildIsAnchor_ReturnsHtmlAnchor()
    {
        var document = HtmlParser.Parse("""<html><body><div><a href="/next?x=1" title="Next">Continue</a></div></body></html>""");
        var element = document.QuerySelector("div");

        var anchor = element.GetAnchor("a");

        Assert.NotNull(anchor);
        var (text, path, query, title, _) = anchor;
        Assert.Equal("Continue", text);
        Assert.EndsWith("/next", path);
        Assert.Equal("1", query["x"]);
        Assert.Equal("Next", title);
    }

    [Fact]
    public void GetAnchor_WhenElementIsNotAnchorAndNoSelectorIsProvided_ReturnsNull()
    {
        var document = HtmlParser.Parse("""<html><body><div>not a link</div></body></html>""");

        var anchor = document.QuerySelector("div").GetAnchor();

        Assert.Null(anchor);
    }

    [Fact]
    public void GetFormData_WhenFormDoesNotExist_ReturnsNull()
    {
        var document = HtmlParser.Parse("""<html><body><div></div></body></html>""");

        var formData = document.Body.GetFormData("form", new Uri("https://example.com"));

        Assert.Null(formData);
    }

    [Fact]
    public void GetFormData_Test()
    {
        const string html = """
                            <!DOCTYPE html>
                            <html>
                            <head>
                                <title>Form with Hidden Inputs</title>
                                <meta charset="UTF-8">
                            </head>
                            <body>
                                <form action="submit.php" method="post">
                                    <input type="hidden" name="user_id" value="12345">
                                    <input type="hidden" name="token" value="abcde12345">
                                    <input type="hidden" name="referrer" value="main_page">
                            
                                    <div>
                                        <label for="username">Username:</label>
                                        <input type="text" id="username" name="username" required>
                                    </div>
                            
                                    <div>
                                        <label for="email">Email:</label>
                                        <input type="email" id="email" name="email" required>
                                    </div>
                            
                                    <input type="submit" value="Submit">
                                </form>
                            </body>
                            </html>
                            """;

        var document = HtmlParser.Parse(html);
        var formData = document.Body.GetFormData("form", new Uri("http://www.example.com"));

        Assert.NotNull(formData);
        Assert.Equal("http://www.example.com/submit.php", formData.SubmitUri.AbsoluteUri);
        Assert.Equal(HttpMethod.Post, formData.Method);
        Assert.Equal(5, formData.Params.Count);
        Assert.Equal("12345", formData.Params["user_id"]);
        Assert.Equal("abcde12345", formData.Params["token"]);
        Assert.Equal("main_page", formData.Params["referrer"]);
        Assert.Equal("", formData.Params["username"]);
        Assert.Equal("", formData.Params["email"]);
    }

    [Fact]
    public void GetFormData_CollectsSuccessfulControlsLikeFormSubmission()
    {
        const string html = """
                            <!DOCTYPE html>
                            <html>
                            <body>
                                <form action="/submit">
                                    <input type="hidden" name="token" value="abc">
                                    <input type="text" name="user" value="alice">
                                    <input type="password" name="password" value="secret">
                                    <input type="checkbox" name="enabled" checked>
                                    <input type="checkbox" name="features" value="a" checked>
                                    <input type="checkbox" name="features" value="b">
                                    <input type="radio" name="role" value="user">
                                    <input type="radio" name="role" value="admin" checked>
                                    <input type="text" value="missing-name">
                                    <input type="text" name="disabled" value="ignored" disabled>
                                    <fieldset disabled>
                                        <legend>
                                            <input type="text" name="legendInput" value="kept">
                                        </legend>
                                        <input type="text" name="fieldsetDisabled" value="ignored">
                                    </fieldset>
                                    <input type="submit" name="submit" value="ignored">
                                    <input type="file" name="upload" value="ignored">
                                    <textarea name="notes">hello</textarea>
                                    <select name="country">
                                        <option value="cn">China</option>
                                        <option value="us" selected>United States</option>
                                    </select>
                                    <select name="tags" multiple>
                                        <option value="red" selected>Red</option>
                                        <option value="blue">Blue</option>
                                        <option selected>Green</option>
                                    </select>
                                    <select name="defaultChoice">
                                        <option value="disabled-first" disabled>Disabled First</option>
                                        <option value="first">First</option>
                                        <option value="second">Second</option>
                                    </select>
                                    <select name="optgroupDisabledChoice">
                                        <optgroup label="disabled group" disabled>
                                            <option value="disabled-selected" selected>Disabled Selected</option>
                                            <option value="disabled-default">Disabled Default</option>
                                        </optgroup>
                                        <option value="enabled-default">Enabled Default</option>
                                    </select>
                                    <select name="optgroupTags" multiple>
                                        <optgroup label="disabled group" disabled>
                                            <option value="disabled-tag" selected>Disabled Tag</option>
                                        </optgroup>
                                        <option value="enabled-tag" selected>Enabled Tag</option>
                                    </select>
                                </form>
                            </body>
                            </html>
                            """;

        var document = HtmlParser.Parse(html);
        var formData = document.Body.GetFormData("form", new Uri("https://www.example.com/page"));

        Assert.NotNull(formData);
        Assert.Equal(HttpMethod.Get, formData.Method);
        Assert.Equal("https://www.example.com/submit", formData.SubmitUri.AbsoluteUri);
        Assert.Equal("abc", formData.Params["token"]);
        Assert.Equal("alice", formData.Params["user"]);
        Assert.Equal("secret", formData.Params["password"]);
        Assert.Equal("on", formData.Params["enabled"]);
        Assert.Equal("a", formData.Params["features"]);
        Assert.Equal("admin", formData.Params["role"]);
        Assert.Equal("hello", formData.Params["notes"]);
        Assert.Equal("us", formData.Params["country"]);
        Assert.Equal(["red", "Green"], formData.Params.GetValues("tags"));
        Assert.Equal("first", formData.Params["defaultChoice"]);
        Assert.Equal("kept", formData.Params["legendInput"]);
        Assert.Equal("enabled-tag", formData.Params["optgroupTags"]);
        Assert.False(formData.Params.ContainsKey("disabled"));
        Assert.False(formData.Params.ContainsKey("fieldsetDisabled"));
        Assert.False(formData.Params.ContainsKey("optgroupDisabledChoice"));
        Assert.False(formData.Params.ContainsKey("submit"));
        Assert.False(formData.Params.ContainsKey("upload"));
    }

    [Fact]
    public void GetFormData_WhenActionIsMissing_UsesCurrentUri()
    {
        const string html = """
                            <html>
                            <body>
                                <form method="post">
                                    <input name="q" value="fclex">
                                </form>
                            </body>
                            </html>
                            """;

        var document = HtmlParser.Parse(html);
        var formData = document.Body.GetFormData("form", new Uri("https://www.example.com/current?x=1"));

        Assert.NotNull(formData);
        Assert.Equal(HttpMethod.Post, formData.Method);
        Assert.Equal("https://www.example.com/current?x=1", formData.SubmitUri.AbsoluteUri);
        Assert.Equal("fclex", formData.Params["q"]);
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

    [Theory]
    [InlineData("""<meta content="0; url=/next" http-equiv="refresh">""", "/next")]
    [InlineData("""<meta http-equiv='REFRESH' content='5; URL="/quoted path"'>""", "/quoted path")]
    [InlineData("""<meta data-x="1" http-equiv="refresh" content="0; Url='https://example.com/a?b=1'">""", "https://example.com/a?b=1")]
    public void GetMetaRefreshUrl_ParsesRefreshMetaTagWithFlexibleHtml(string metaTag, string expected)
    {
        var document = HtmlParser.Parse($"""
                                        <html>
                                        <head>{metaTag}</head>
                                        <body></body>
                                        </html>
                                        """);

        var actual = document.DocumentElement.GetMetaRefreshUrl();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetMetaRefreshUrl_WhenRefreshMetaTagHasNoUrl_ReturnsNull()
    {
        var document = HtmlParser.Parse("""
                                        <html>
                                        <head><meta http-equiv="refresh" content="5"></head>
                                        <body></body>
                                        </html>
                                        """);

        var actual = document.DocumentElement.GetMetaRefreshUrl();

        Assert.Null(actual);
    }

    private static string HtmlEncode(string value) => WebUtility.HtmlEncode(value);
}
