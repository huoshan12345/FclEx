namespace FclEx.Http.Actions;

public class ExtensionsTests
{
    [Fact]
    public async Task ReadJsonAs_ReadsPathFromSuccessfulHttpResponseAction()
    {
        var response = HttpActionTestFixtures.CreateResponse("""{"data":{"name":"fclex"}}""");
        var action = Operation.SuccessAction(response).ReadJsonAs<string>("data.name");

        var result = await action.ExecuteAsync();

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Equal("fclex", result.Value);
    }

    [Fact]
    public async Task ReadJsonAs_WhenResponseContainsException_PropagatesException()
    {
        var exception = new InvalidOperationException("failed");
        var response = HttpActionTestFixtures.CreateResponse(exception: exception, elapsed: HttpActionTestFixtures.Elapsed);
        var action = Operation.SuccessAction(response).ReadJsonAs<int>();

        var result = await action.ExecuteAsync();

        Assert.True(result.IsError);
        Assert.Same(exception, result.Exception);
        Assert.Equal(HttpActionTestFixtures.Elapsed, result.Elapsed);
    }

    [Fact]
    public async Task ThenRequest_UsesFactoryResultAndProvidedHttpService()
    {
        var response = HttpActionTestFixtures.CreateResponse("next");
        var service = new StubHttpService(response);
        var action = Operation.SuccessAction("id")
            .ThenRequest(id => HttpRequest.Post("https://example.com/" + id), service);

        var result = await action.ExecuteAsync();

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Same(response, result.Value);
        Assert.Single(service.Requests);
        Assert.Equal(HttpMethod.Post, service.Requests[0].Method);
        Assert.Equal("https://example.com/id", service.Requests[0].GetUri().ToString());
    }

    [Fact]
    public async Task ThenRequest_WithStaticRequest_ReusesProvidedRequest()
    {
        var response = HttpActionTestFixtures.CreateResponse("next");
        var service = new StubHttpService(response);
        var request = HttpRequest.Post("https://example.com/static");
        var action = Operation.SuccessAction("ignored").ThenRequest(request, service);

        var result = await action.ExecuteAsync();

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Single(service.Requests);
        Assert.Same(request, service.Requests[0]);
    }

    [Fact]
    public async Task ThenRequest_WhenSourceFails_DoesNotCreateNextRequest()
    {
        var service = new StubHttpService(HttpActionTestFixtures.CreateResponse());
        var called = false;
        var action = Operation.ErrorAction<string>("stop")
            .ThenRequest(_ =>
            {
                called = true;
                return HttpRequest.Get("https://example.com/next");
            }, service);

        var result = await action.ExecuteAsync();

        Assert.True(result.IsError);
        Assert.False(called);
        Assert.Empty(service.Requests);
    }

    [Fact]
    public void ThenRequest_WhenFactoryIsNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            Operation.SuccessAction("id").ThenRequest((Func<string, HttpRequest>)null!));
    }

    [Fact]
    public void ThenRequest_WhenRequestIsNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            Operation.SuccessAction("id").ThenRequest((HttpRequest)null!));
    }

    [Fact]
    public void TryCreateRedirectAction_WhenUrlIsNull_ReturnsNull()
    {
        var response = HttpActionTestFixtures.CreateResponse();

        var action = response.TryCreateRedirectAction(new StubHttpService(HttpActionTestFixtures.CreateResponse()), static _ => null);

        Assert.Null(action);
    }

    [Fact]
    public async Task TryCreateRedirectAction_WhenUrlExists_CreatesGetAction()
    {
        var redirected = HttpActionTestFixtures.CreateResponse("redirected");
        var service = new StubHttpService(redirected);
        var action = HttpActionTestFixtures.CreateResponse().TryCreateRedirectAction(service, "https://example.com/redirect");

        Assert.NotNull(action);

        var result = await action!.ExecuteAsync();

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Same(redirected, result.Value);
        Assert.Single(service.Requests);
        Assert.Equal(HttpMethod.Get, service.Requests[0].Method);
        Assert.Equal("https://example.com/redirect", service.Requests[0].GetUri().ToString());
    }

    [Fact]
    public void TryCreateRedirectAction_PassesResponseToUrlFactory()
    {
        var response = HttpActionTestFixtures.CreateResponse();
        HttpResponse? captured = null;

        var action = response.TryCreateRedirectAction(new StubHttpService(HttpActionTestFixtures.CreateResponse()), r =>
        {
            captured = r;
            return null;
        });

        Assert.Null(action);
        Assert.Same(response, captured);
    }

    [Fact]
    public void TryCreateRedirectAction_WhenUrlFactoryIsNull_Throws()
    {
        var response = HttpActionTestFixtures.CreateResponse();

        Assert.Throws<ArgumentNullException>(() =>
            response.TryCreateRedirectAction(new StubHttpService(HttpActionTestFixtures.CreateResponse()), (Func<HttpResponse, string?>)null!));
    }
}
