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
    public void ModifyRequest_WhenCallbackNameIsCustom_AddsConfiguredCallbackName()
    {
        var action = new TestJsonpAction { CallbackNameValue = "jsonp123" };
        var request = HttpRequest.Get("https://example.com/api");

        action.ModifyRequest(request);

        Assert.Equal("https://example.com/api?cb=jsonp123", request.GetUri().ToString());
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
    public void GetJson_WhenCallbackNameIsCustom_ExtractsCallbackBody()
    {
        var response = HttpActionTestFixtures.CreateResponse("""jsonp123({"value":5})""");
        var action = new TestJsonpAction { CallbackNameValue = "jsonp123" };

        var result = action.GetJson(response);

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Equal("""{"value":5}""", result.Value);
    }

    [Fact]
    public void GetJson_WhenCallbackHasWhitespaceAndTrailingSemicolon_ExtractsCallbackBody()
    {
        var response = HttpActionTestFixtures.CreateResponse("""  _callback ( {"value":5} ) ;  """);
        var action = new TestJsonpAction();

        var result = action.GetJson(response);

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Equal("""{"value":5}""", result.Value);
    }

    [Fact]
    public void GetJson_WhenJsonContainsClosingParenthesis_ExtractsWholeCallbackBody()
    {
        var response = HttpActionTestFixtures.CreateResponse("""_callback({"text":")"})""");
        var action = new TestJsonpAction();

        var result = action.GetJson(response);

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Equal("""{"text":")"}""", result.Value);
    }

    [Fact]
    public void GetJson_WhenCallbackIsMissing_ReturnsError()
    {
        var response = HttpActionTestFixtures.CreateResponse("""{"value":5}""");
        var action = new TestJsonpAction();

        var result = action.GetJson(response);

        Assert.True(result.IsError);
        Assert.Contains("Failed to parse JSONP callback", result.Exception!.Message);
    }

    [Fact]
    public void GetJson_WhenCallbackNameDoesNotMatch_ReturnsError()
    {
        var response = HttpActionTestFixtures.CreateResponse("""other({"value":5})""");
        var action = new TestJsonpAction();

        var result = action.GetJson(response);

        Assert.True(result.IsError);
        Assert.Contains("Failed to parse JSONP callback", result.Exception!.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("_callback()")]
    [InlineData("_callback(   )")]
    [InlineData("_callback {\"value\":5}")]
    [InlineData("_callback({\"value\":5}")]
    public void GetJson_WhenCallbackWrapperIsMalformed_ReturnsError(string responseString)
    {
        var response = HttpActionTestFixtures.CreateResponse(responseString);
        var action = new TestJsonpAction();

        var result = action.GetJson(response);

        Assert.True(result.IsError);
        Assert.Contains("Failed to parse JSONP callback", result.Exception!.Message);
    }

    [Fact]
    public void GetJson_WhenCallbackNameIsEmpty_ReturnsError()
    {
        var response = HttpActionTestFixtures.CreateResponse("""({"value":5})""");
        var action = new TestJsonpAction { CallbackNameValue = "" };

        var result = action.GetJson(response);

        Assert.True(result.IsError);
        Assert.Contains("Failed to parse JSONP callback", result.Exception!.Message);
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
