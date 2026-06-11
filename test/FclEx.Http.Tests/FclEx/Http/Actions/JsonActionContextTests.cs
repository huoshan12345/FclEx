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
        Assert.True(context.TryGetResultToken(out var token));
        Assert.Equal(42, token.GetProperty("value").GetInt32());
    }

    [Fact]
    public void ResultTokens_WhenParserDocumentIsDisposed_RemainReadable()
    {
        var response = HttpActionTestFixtures.CreateResponse();
        var context = new JsonActionContext(response, """{"items":[{"id":1},{"id":2}]}""", "items[*].id");

        Assert.Equal([1, 2], context.ResultTokens.Select(token => token.GetInt32()));
    }

    [Fact]
    public void Token_WhenParserDocumentIsDisposed_RemainsReadable()
    {
        var response = HttpActionTestFixtures.CreateResponse();
        var context = new JsonActionContext(response, """{"data":{"count":3}}""", "data.count");

        Assert.Equal(3, context.Token.GetProperty("data").GetProperty("count").GetInt32());
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

        var found = context.TryGetResultToken(out var token);

        Assert.False(found);
        Assert.Equal(default, token);
    }

    [Fact]
    public void TryGetResultToken_WhenPathMatches_ReturnsFirstToken()
    {
        var response = HttpActionTestFixtures.CreateResponse();
        var context = new JsonActionContext(response, """{"items":[{"id":1},{"id":2}]}""", "items[*].id");

        var found = context.TryGetResultToken(out var token);

        Assert.True(found);
        Assert.Equal(1, token.GetInt32());
    }
}
