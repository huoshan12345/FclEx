namespace FclEx.Http.Actions;

public class DefaultHttpActionTests
{
    [Fact]
    public async Task ExecuteCoreAsync_WhenEnsureSuccessStatusCodeIsTrue_RejectsUnsuccessfulStatusCode()
    {
        var response = HttpActionTestFixtures.CreateResponse("missing", HttpStatusCode.NotFound, HttpActionTestFixtures.Elapsed);
        var action = new PipelineHttpAction<string>(response)
        {
            EnsureSuccessStatusCodeValue = true,
            Result = "should-not-run"
        };

        var result = await action.ExecuteAsync();

        Assert.True(result.IsError);
        Assert.True(result.Elapsed < HttpActionTestFixtures.Elapsed); // result.Elapsed is accurately measured and should be less than the response's elapsed time
        Assert.Contains("NotFound", result.Exception.Message);
        Assert.Equal(0, action.GetResultCallCount);
    }

    [Fact]
    public async Task ExecuteCoreAsync_WhenEnsureSuccessStatusCodeIsFalse_AllowsUnsuccessfulStatusCode()
    {
        var response = HttpActionTestFixtures.CreateResponse("missing", HttpStatusCode.NotFound, HttpActionTestFixtures.Elapsed);
        var action = new PipelineHttpAction<string>(response)
        {
            EnsureSuccessStatusCodeValue = false,
            Result = "accepted"
        };

        var result = await action.ExecuteAsync();

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Equal("accepted", result.Value);
        Assert.Equal(1, action.GetResultCallCount);
    }

    [Fact]
    public async Task ExecuteCoreAsync_WhenStatusCodeIsSuccess_RunsResultHandler()
    {
        var response = HttpActionTestFixtures.CreateResponse("ok", HttpStatusCode.NoContent, HttpActionTestFixtures.Elapsed);
        var action = new PipelineHttpAction<string>(response)
        {
            EnsureSuccessStatusCodeValue = true,
            Result = "handled"
        };

        var result = await action.ExecuteAsync();

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Equal("handled", result.Value);
        Assert.Equal(1, action.GetResultCallCount);
    }

    [Fact]
    public async Task ExecuteCoreAsync_WhenResponseHasException_ReturnsResponseExceptionWithoutHandlingResult()
    {
        var exception = new InvalidOperationException("send failed");
        var response = HttpActionTestFixtures.CreateResponse("failed", elapsed: HttpActionTestFixtures.Elapsed, exception: exception);
        var action = new PipelineHttpAction<string>(response) { Result = "should-not-run" };

        var result = await action.ExecuteAsync();

        Assert.True(result.IsError);
        Assert.Same(exception, result.Exception);
        Assert.True(result.Elapsed < HttpActionTestFixtures.Elapsed); // result.Elapsed is accurately measured and should be less than the response's elapsed time
        Assert.Equal(0, action.GetResultCallCount);
    }

    [Fact]
    public async Task ExecuteCoreAsync_WhenGetResponseAsyncThrows_ReturnsError()
    {
        var exception = new InvalidOperationException("boom");
        var action = new PipelineHttpAction<string>(HttpActionTestFixtures.CreateResponse())
        {
            GetResponseException = exception,
            Result = "should-not-run"
        };

        var result = await action.ExecuteAsync();

        Assert.True(result.IsError);
        Assert.Same(exception, result.Exception);
        Assert.Equal(0, action.GetResultCallCount);
    }

    [Fact]
    public void BuildRequest_DisablesHttpRequestEnsureSuccessAndAppliesModification()
    {
        var action = new PipelineHttpAction<Unit>(HttpActionTestFixtures.CreateResponse())
        {
            UriValue = new Uri("https://example.com/api"),
            MethodValue = HttpMethod.Post
        };

        var request = action.BuildRequest();

        Assert.Equal(action.UriValue, request.GetUri());
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.False(request.EnsureSuccessStatusCode);
        Assert.Equal("yes", request.Headers.Get("X-Modified"));
        Assert.Equal(1, action.ModifyRequestCallCount);
    }

    [Fact]
    public async Task ExecuteCoreAsync_PassesCancellationTokenToGetResponseAsync()
    {
        var action = new PipelineHttpAction<Unit>(HttpActionTestFixtures.CreateResponse()) { Result = Unit.Default };
        using var cts = new CancellationTokenSource();

        var result = await action.ExecuteAsync(cts.Token);

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Equal(cts.Token, action.LastToken);
    }
}
