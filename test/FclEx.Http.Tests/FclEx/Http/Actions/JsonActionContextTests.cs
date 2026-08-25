namespace FclEx.Http.Actions;

public class JsonActionContextTests
{
    [Fact]
    public void Constructor_WhenPathIsNull_SelectsRootTokenAndKeepsResponseAndJson()
    {
        var response = HttpActionTestFixtures.CreateResponse();

        var context = new JsonActionContext(response, """{"value":42}""", null);

        Assert.Same(response, context.Response);
        Assert.Null(context.JsonPath);
        Assert.Equal("""{"value":42}""", context.Json);
        Assert.Single(context.ResultTokens);
        Assert.Equal(42, context.ResultToken?["value"]?.GetValue<int>());
    }

    [Fact]
    public void Constructor_WhenPathMatchesNothing_KeepsRootTokenAndEmptyResultTokens()
    {
        var response = HttpActionTestFixtures.CreateResponse();

        var context = new JsonActionContext(response, """{"value":42}""", "missing");

        Assert.Equal(42, context.ResultToken?["value"]?.GetValue<int>());
        Assert.Empty(context.ResultTokens);
        Assert.Null(context.ResultToken);
    }

    [Fact]
    public void ResultTokens_WhenParserDocumentIsDisposed_RemainReadable()
    {
        var response = HttpActionTestFixtures.CreateResponse();
        var context = new JsonActionContext(response, """{"items":[{"id":1},{"id":2}]}""", "items[*].id");

        Assert.Equal([1, 2], context.ResultTokens.Select(token => token?.GetValue<int>()));
    }

    [Fact]
    public void Token_WhenParserDocumentIsDisposed_RemainsReadable()
    {
        var response = HttpActionTestFixtures.CreateResponse();
        var context = new JsonActionContext(response, """{"data":{"count":3}}""", "data.count");

        Assert.Equal(3, context.ResultToken?["data"]?["count"]?.GetValue<int>());
    }

    [Fact]
    public void ResultToken_WhenPathDoesNotMatch_ReturnsNull()
    {
        var response = HttpActionTestFixtures.CreateResponse();
        var context = new JsonActionContext(response, """{"data":{"count":3}}""", "missing.count");

        var resultToken = context.ResultToken;

        Assert.Null(resultToken);
    }

    [Fact]
    public void TryGetResultToken_WhenPathDoesNotMatch_ReturnsFalse()
    {
        var response = HttpActionTestFixtures.CreateResponse();
        var context = new JsonActionContext(response, """{"data":{"count":3}}""", "missing.count");
        Assert.Equal(default, context.ResultToken);
    }

    [Fact]
    public void TryGetResultToken_WhenPathMatches_ReturnsFirstToken()
    {
        var response = HttpActionTestFixtures.CreateResponse();
        var context = new JsonActionContext(response, """{"items":[{"id":1},{"id":2}]}""", "items[*].id");
        Assert.Equal(1, context.ResultToken?.GetValue<int>());
    }
}
