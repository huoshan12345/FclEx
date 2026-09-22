namespace FclEx.Http.Actions;

public class DefaultHtmlActionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GetResult_WhenConversionCompletes_DisposesDocument(bool returnError)
    {
        var response = HttpActionTestFixtures.CreateResponse("<p>content</p>");
        var action = new TrackingHtmlAction(returnError, false);

        var result = DefaultHtmlAction.GetResult(action, response);

        Assert.Equal(returnError, result.IsError);
        if (!returnError)
            Assert.Equal("content", result.Value);
        Assert.NotNull(action.Document);
        Assert.Null(action.Document.DocumentElement);
    }

    [Fact]
    public void GetResult_WhenConversionThrows_DisposesDocumentAndPropagatesException()
    {
        var response = HttpActionTestFixtures.CreateResponse("<p>content</p>");
        var action = new TrackingHtmlAction(false, true);

        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            DefaultHtmlAction.GetResult(action, response);
        });

        Assert.Same(action.Failure, exception);
        Assert.NotNull(action.Document);
        Assert.Null(action.Document.DocumentElement);
    }

    [Fact]
    public void CreateContext_WhenSuccessful_TransfersLiveDocumentToCaller()
    {
        var response = HttpActionTestFixtures.CreateResponse("<p>content</p>");
        var result = DefaultHtmlAction.CreateContext(new HtmlTextAction(), response, response.ResponseString!);

        Assert.True(result.IsSuccess);
        using var context = result.Value;
        Assert.NotNull(context.Document.DocumentElement);
        Assert.Equal("content", context.Document.Body!.TextContent);
    }

    private sealed class TrackingHtmlAction(bool returnError, bool throwException) : HtmlAction<string>
    {
        public AngleSharp.Html.Dom.IHtmlDocument? Document { get; private set; }

        public InvalidOperationException Failure { get; } = new("Conversion failed");

        public override OperationResult<string> GetResult(HtmlActionContext context)
        {
            Document = context.Document;
            Assert.NotNull(Document.DocumentElement);
            if (throwException)
                throw Failure;

            return returnError
                ? Operation.Error<string>("Conversion failed")
                : Operation.Success(Document.Body!.TextContent);
        }
    }
}
