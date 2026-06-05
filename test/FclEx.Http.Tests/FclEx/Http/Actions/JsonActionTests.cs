namespace FclEx.Http.Actions;

public class JsonActionTests
{
    [Fact]
    public void GetResult_WhenPathMatches_DeserializesResultToken()
    {
        var response = HttpActionTestFixtures.CreateResponse("""{"data":{"count":3}}""");
        var action = new JsonCountAction { JsonPathValue = "data.count" };

        var result = action.GetResult(response);

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Equal(3, result.Value);
    }

    [Fact]
    public void GetResult_WhenPathIsNull_DeserializesRootToken()
    {
        var response = HttpActionTestFixtures.CreateResponse("3");
        var action = new JsonCountAction();

        var result = action.GetResult(response);

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Equal(3, result.Value);
    }

    [Fact]
    public void GetResult_WhenRootIsJsonString_DeserializesStringValue()
    {
        var response = HttpActionTestFixtures.CreateResponse("\"hello\"");
        var action = new JsonStringAction();

        var result = action.GetResult(response);

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Equal("hello", result.Value);
    }

    [Fact]
    public void GetResult_WhenPathDoesNotMatch_ReturnsError()
    {
        var response = HttpActionTestFixtures.CreateResponse("""{"data":{"count":3}}""");
        var action = new JsonCountAction { JsonPathValue = "missing.count" };

        var result = action.GetResult(response);

        Assert.True(result.IsError);
        Assert.Contains("missing.count", result.Exception!.Message);
    }

    [Fact]
    public void GetResult_WhenResponseIsNotJson_ReturnsError()
    {
        var response = HttpActionTestFixtures.CreateResponse("not json");
        var action = new JsonCountAction();

        var result = action.GetResult(response);

        Assert.True(result.IsError);
        Assert.Contains("not a valid json", result.Exception!.Message);
    }

    [Fact]
    public void GetResult_ForUnitAction_ReturnsSuccessWithoutReadingPayloadValue()
    {
        var response = HttpActionTestFixtures.CreateResponse("""{"ok":true}""");
        var action = new UnitJsonAction();

        var result = action.GetResult(response);

        Assert.True(result.IsSuccess, result.Exception?.ToString());
    }

    [Fact]
    public void CreateContext_WhenJsonIsValidButPathIsMissing_ReturnsError()
    {
        var response = HttpActionTestFixtures.CreateResponse();
        var action = new JsonCountAction { JsonPathValue = "missing" };

        var result = action.CreateContext(response, """{"value":1}""");

        Assert.True(result.IsError);
        Assert.Contains("missing", result.Exception!.Message);
    }

    [Fact]
    public void CreateContext_UsesJsonPathToSelectResultTokens()
    {
        var response = HttpActionTestFixtures.CreateResponse();
        var action = new JsonCountAction { JsonPathValue = "items[*].id" };

        var result = action.CreateContext(response, """{"items":[{"id":1},{"id":2}]}""");

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Equal("items[*].id", result.Value!.JsonPath);
        Assert.Equal([1, 2], result.Value.ResultTokens.Select(token => token.GetInt32()));
    }

    [Fact]
    public void CreateContext_WhenJsonParsingFails_Throws()
    {
        var response = HttpActionTestFixtures.CreateResponse();
        var action = new JsonCountAction();

        Assert.ThrowsAny<JsonException>(() => (object)action.CreateContext(response, "{ invalid-json }"));
    }

    [Fact]
    public async Task HttpJsonAction_WhenJsonParsingThrows_IsCaughtByPipeline()
    {
        var response = HttpActionTestFixtures.CreateResponse("{ invalid-json }");
        var action = new PipelineJsonAction<int>(response);

        var result = await action.ExecuteAsync();

        Assert.True(result.IsError);
        Assert.IsType<JsonException>(result.Exception, false);
    }
}
