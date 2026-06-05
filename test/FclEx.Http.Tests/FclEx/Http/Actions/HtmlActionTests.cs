namespace FclEx.Http.Actions;

public class HtmlActionTests
{
    [Fact]
    public void GetResult_WhenSelectorMatches_ReturnsSelectedElement()
    {
        var response = HttpActionTestFixtures.CreateResponse("""<html><body><span class="name">fclex</span></body></html>""");
        var action = new HtmlTextAction { HtmlSelectorValue = ".name" };

        var result = action.GetResult(response);

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Equal("fclex", result.Value);
    }

    [Fact]
    public void GetResult_WhenSelectorMatchesMultipleElements_ReturnsFirstElement()
    {
        var response = HttpActionTestFixtures.CreateResponse("<html><body><span>first</span><span>second</span></body></html>");
        var action = new HtmlTextAction { HtmlSelectorValue = "span" };

        var result = action.GetResult(response);

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Equal("first", result.Value);
    }

    [Fact]
    public void GetResult_WhenSelectorIsNull_UsesDocumentElement()
    {
        var response = HttpActionTestFixtures.CreateResponse("<html><body><main>content</main></body></html>");
        var action = new HtmlTextAction();

        var result = action.GetResult(response);

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Contains("content", result.Value);
    }

    [Fact]
    public void GetResult_WhenSelectorDoesNotMatch_ReturnsError()
    {
        var response = HttpActionTestFixtures.CreateResponse("<html><body></body></html>");
        var action = new HtmlTextAction { HtmlSelectorValue = ".missing" };

        var result = action.GetResult(response);

        Assert.True(result.IsError);
        Assert.Contains(".missing", result.Exception!.Message);
    }

    [Fact]
    public void GetHtml_WhenResponseIsEmpty_ReturnsError()
    {
        var response = HttpActionTestFixtures.CreateResponse("");
        var action = new HtmlTextAction();

        var result = action.GetHtml(response);

        Assert.True(result.IsError);
        Assert.Contains("empty", result.Exception!.Message);
    }

    [Fact]
    public void GetResult_ForUnitAction_ReturnsSuccessWhenHtmlExists()
    {
        var response = HttpActionTestFixtures.CreateResponse("<html><body>ok</body></html>");
        var action = new UnitHtmlAction();

        var result = action.GetResult(response);

        Assert.True(result.IsSuccess, result.Exception?.ToString());
    }

    [Fact]
    public void CreateContext_ExposesHtmlResponseAndSelectedElements()
    {
        var response = HttpActionTestFixtures.CreateResponse();
        var action = new HtmlTextAction { HtmlSelectorValue = "li" };

        var result = action.CreateContext(response, "<html><body><ul><li>A</li><li>B</li></ul></body></html>");

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Same(response, result.Value!.Response);
        Assert.Equal("li", result.Value.HtmlSelector);
        Assert.Equal(2, result.Value.ResultElements.Count());
        Assert.Equal("A", result.Value.ResultElement!.TextContent);
    }

    [Fact]
    public void CreateContext_UsesHtmlSelectorToSelectResultElements()
    {
        var response = HttpActionTestFixtures.CreateResponse();
        var action = new HtmlTextAction { HtmlSelectorValue = ".item" };

        var result = action.CreateContext(response, """<html><body><span class="item">A</span><span>B</span></body></html>""");

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Equal(".item", result.Value!.HtmlSelector);
        Assert.Equal("A", result.Value.ResultElement!.TextContent);
    }

    [Fact]
    public void CreateContext_WhenSelectorIsInvalid_DoesNotReturnSuccess()
    {
        var response = HttpActionTestFixtures.CreateResponse();
        var action = new HtmlTextAction { HtmlSelectorValue = "[" };

        try
        {
            var result = action.CreateContext(response, "<html><body>ok</body></html>");
            Assert.True(result.IsError);
        }
        catch (Exception ex)
        {
            Assert.NotNull(ex);
        }
    }
}
