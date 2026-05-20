namespace FclEx.Http.Actions;

public class JsonpActionTests
{
    [Fact]
    public void ModifyRequest_AddsConfiguredCallbackParameter()
    {
        var action = new TestJsonpAction();
        var request = HttpRequest.Get("https://example.com/api");

        action.ModifyRequest(request);

        Assert.Equal("https://example.com/api?cb=_callback", request.GetUri().ToString());
    }

    [Fact]
    public void ModifyRequest_PreservesExistingQuery()
    {
        var action = new TestJsonpAction();
        var request = HttpRequest.Get("https://example.com/api?x=1");

        action.ModifyRequest(request);

        Assert.Equal("https://example.com/api?x=1&cb=_callback", request.GetUri().ToString());
    }

    [Fact]
    public void GetJson_ExtractsCallbackBody()
    {
        var response = HttpActionTestFixtures.CreateResponse("""_callback({"value":5})""");
        var action = new TestJsonpAction();

        var result = action.GetJson(response);

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Equal("""{"value":5}""", result.Value);
    }

    [Fact]
    public void GetJson_WhenCallbackIsMissing_ReturnsError()
    {
        var response = HttpActionTestFixtures.CreateResponse("""{"value":5}""");
        var action = new TestJsonpAction();

        var result = action.GetJson(response);

        Assert.True(result.IsError);
        Assert.Contains("Failed to parse callback", result.Exception!.Message);
    }

    [Fact]
    public void GetResult_DeserializesExtractedCallbackBody()
    {
        var response = HttpActionTestFixtures.CreateResponse("""_callback({"value":5})""");
        var action = new TestJsonpAction();

        var result = action.GetResult(response);

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Equal(5, result.Value.GetProperty("value").GetInt32());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCallbackBodyIsInvalidJson_ReturnsError()
    {
        var response = HttpActionTestFixtures.CreateResponse("_callback({ invalid-json })");
        var action = new TestJsonpAction(response);

        var result = await action.ExecuteAsync();

        Assert.True(result.IsError);
        Assert.IsType<JsonException>(result.Exception, false);
    }
}
