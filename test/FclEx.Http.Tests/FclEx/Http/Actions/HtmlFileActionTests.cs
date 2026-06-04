namespace FclEx.Http.Actions;

public class HtmlFileActionTests
{
    [Fact]
    public async Task ExecuteAsync_ReadsFileAndUsesSelector()
    {
        var path = HttpActionTestFixtures.CreateTempFile("<html><body><h1>Title</h1></body></html>");
        var action = new TestHtmlFileAction(path) { HtmlSelectorValue = "h1" };

        try
        {
            var result = await action.ExecuteAsync();

            Assert.True(result.IsSuccess, result.Exception?.ToString());
            Assert.Equal("Title", result.Value);
            Assert.Equal(new Uri(Path.GetFullPath(path)), action.Uri);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task ExecuteAsync_WhenSelectorIsNull_UsesDocumentElement()
    {
        var path = HttpActionTestFixtures.CreateTempFile("<html><body><main>file content</main></body></html>");
        var action = new TestHtmlFileAction(path);

        try
        {
            var result = await action.ExecuteAsync();

            Assert.True(result.IsSuccess, result.Exception?.ToString());
            Assert.Contains("file content", result.Value);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task ExecuteAsync_WhenReadFileThrows_ReturnsError()
    {
        var action = new TestHtmlFileAction(Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".html"));

        var result = await action.ExecuteAsync();

        Assert.True(result.IsError);
        Assert.IsAssignableFrom<IOException>(result.Exception);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCreateContextThrows_IsCaughtByPipeline()
    {
        var path = HttpActionTestFixtures.CreateTempFile("<html><body><h1>Title</h1></body></html>");
        var action = new TestHtmlFileAction(path) { HtmlSelectorValue = "[" };

        try
        {
            var result = await action.ExecuteAsync();

            Assert.True(result.IsError);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task GetResponseAsync_ReturnsOkResponseWithFileContent()
    {
        var path = HttpActionTestFixtures.CreateTempFile("<html><body>raw</body></html>");
        var action = new TestHtmlFileAction(path);
        var request = action.BuildRequest();

        try
        {
            var response = await action.GetResponseAsync(request);

            Assert.False(response.IsError);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("<html><body>raw</body></html>", response.ResponseString);
            Assert.Same(request, response.Request);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
