namespace FclEx.Http.Actions;

public class HttpRequestActionUnitTests
{
    [Fact]
    public async Task ExecuteAsync_WhenResponseSucceeds_PreservesElapsed()
    {
        var response = HttpActionTestFixtures.CreateResponse("{\"value\":1}", elapsed: HttpActionTestFixtures.Elapsed);
        var action = HttpRequest.Get("https://example.com/api").ToAction(new StubHttpService(response));

        var result = await action.ExecuteAsync();

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Same(response, result.Value);
        Assert.Equal(HttpActionTestFixtures.Elapsed, result.Elapsed);
    }

    [Fact]
    public async Task ExecuteAsync_WhenUnwrapErrorIsTrue_ReturnsObjectErrorWithElapsed()
    {
        var failure = new InvalidOperationException("network failed");
        var response = HttpActionTestFixtures.CreateResponse("failed", elapsed: HttpActionTestFixtures.Elapsed, exception: failure);
        var action = HttpRequest.Get("https://example.com/api").ToAction(new StubHttpService(response));

        var result = await action.ExecuteAsync();

        Assert.True(result.IsError);
        Assert.Equal(HttpActionTestFixtures.Elapsed, result.Elapsed);
        Assert.True(result.Exception.IsObjectException<HttpResponse>(r => ReferenceEquals(r, response)));
        Assert.Same(failure, result.Exception!.InnerException);
    }

    [Fact]
    public async Task ExecuteAsync_WhenUnwrapErrorIsFalse_ReturnsResponseAsSuccessfulValue()
    {
        var response = HttpActionTestFixtures.CreateResponse("failed", elapsed: HttpActionTestFixtures.Elapsed, exception: new InvalidOperationException("network failed"));
        var action = HttpRequest.Get("https://example.com/api").ToAction(new StubHttpService(response), unwrapError: false);

        var result = await action.ExecuteAsync();

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Same(response, result.Value);
        Assert.True(result.Value!.IsError);
        Assert.Equal(HttpActionTestFixtures.Elapsed, result.Elapsed);
    }

    [Fact]
    public async Task ExecuteAsync_PassesRequestAndCancellationTokenToHttpService()
    {
        var response = HttpActionTestFixtures.CreateResponse("ok");
        var service = new StubHttpService(response);
        var request = HttpRequest.Post("https://example.com/api");
        using var cts = new CancellationTokenSource();
        var action = request.ToAction(service);

        var result = await action.ExecuteAsync(cts.Token);

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Single(service.Requests);
        Assert.Same(request, service.Requests[0]);
        Assert.Single(service.Tokens);
        Assert.Equal(cts.Token, service.Tokens[0]);
    }
}
