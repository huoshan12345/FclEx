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
    public async Task HandleResponseAsync_WhenStatusCodeIsSuccessful_ReturnsSuccessWithResponseElapsed()
    {
        var response = HttpActionTestFixtures.CreateResponse("ok", HttpStatusCode.OK, HttpActionTestFixtures.Elapsed);
        var action = new PipelineHttpAction<string>(response)
        {
            EnsureSuccessStatusCodeValue = true,
        };

        var result = await action.HandleResponseAsync(response);

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Same(response, result.Value);
        Assert.Equal(HttpActionTestFixtures.Elapsed, result.Elapsed);
    }

    [Fact]
    public async Task HandleResponseAsync_WhenSuccessEnforcementIsDisabled_ReturnsSuccessForFailureStatus()
    {
        var response = HttpActionTestFixtures.CreateResponse("missing", HttpStatusCode.NotFound, HttpActionTestFixtures.Elapsed);
        var action = new PipelineHttpAction<string>(response)
        {
            EnsureSuccessStatusCodeValue = false,
        };

        var result = await action.HandleResponseAsync(response);

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Same(response, result.Value);
        Assert.Equal(HttpActionTestFixtures.Elapsed, result.Elapsed);
    }

    [Fact]
    public async Task HandleResponseAsync_WhenStatusCodeIsUnsuccessful_TruncatesResponseStringInError()
    {
        var response = HttpActionTestFixtures.CreateResponse(new string('x', 300), HttpStatusCode.BadGateway, HttpActionTestFixtures.Elapsed);
        var action = new PipelineHttpAction<string>(response)
        {
            EnsureSuccessStatusCodeValue = true,
        };

        var result = await action.HandleResponseAsync(response);

        Assert.True(result.IsError);
        Assert.Equal(HttpActionTestFixtures.Elapsed, result.Elapsed);
        Assert.Contains("BadGateway/502", result.Exception!.Message);
        Assert.True(result.Exception.Message.Length < 420);
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
#if NET5_0_OR_GREATER
        Assert.Equal("gzip, deflate, br", request.Headers.Get(HttpHeaderNames.AcceptEncoding));
#else
        Assert.Equal("gzip", request.Headers.Get(HttpHeaderNames.AcceptEncoding));
#endif
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
