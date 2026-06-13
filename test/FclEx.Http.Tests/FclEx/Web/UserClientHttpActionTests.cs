namespace FclEx.Web;

public class UserClientHttpActionTests : WebTests
{
    [Fact]
    public void Constructor_ExposesClientStateSessionAccountAndLogger()
    {
        var client = new TestUserClient(ServiceProvider.GetRequiredService<ILoggerFactory>())
        {
            Account = new UserAccount("alice", "pwd"),
        };
        var action = new InspectableUserClientHttpAction(client);

        Assert.Same(client, action.Client);
        Assert.Same(client.State, action.State);
        Assert.Same(client.Session, action.Session);
        Assert.Same(client.Account, action.Account);
        Assert.Same(client.Logger, action.Logger);
        Assert.Same(client.HttpService, action.HttpService);
    }

    [Fact]
    public void BuildRequest_UsesActionUriMethodAndDisablesTransportSuccessEnforcement()
    {
        var client = new TestUserClient(ServiceProvider.GetRequiredService<ILoggerFactory>());
        var action = new InspectableUserClientHttpAction(client);

        var request = action.BuildRequest();

        Assert.Equal(action.Uri, request.GetUri());
        Assert.Equal(action.Method, request.Method);
        Assert.False(request.EnsureSuccessStatusCode);
#if NET5_0_OR_GREATER
        Assert.Equal("gzip, deflate, br", request.Headers.Get(HttpHeaderNames.AcceptEncoding));
#else
        Assert.Equal("gzip", request.Headers.Get(HttpHeaderNames.AcceptEncoding));
#endif
    }

    [Fact]
    public async Task ExecuteAsync_WhenEnsureSuccessStatusCodeIsOverridden_AllowsUnsuccessfulResponse()
    {
        var client = new TestUserClient(ServiceProvider.GetRequiredService<ILoggerFactory>());
        var action = new NonEnforcingUserClientHttpAction(client);

        var result = await ((FclEx.Actions.IAction<string>)action).ExecuteAsync();

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Equal("accepted", result.Value);
    }

    [Fact]
    public async Task ExecuteAsync_UsesClientHttpServiceAndPassesCancellationToken()
    {
        var response = CreateResponse("handled");
        var service = new CaptureHttpService(response);
        var client = new TestUserClient(ServiceProvider.GetRequiredService<ILoggerFactory>())
        {
            HttpService = service,
        };
        var action = new ServiceBackedUserClientHttpAction(client);
        using var cts = new CancellationTokenSource();

        var result = await ((FclEx.Actions.IAction<string>)action).ExecuteAsync(cts.Token);

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Equal("handled", result.Value);
        Assert.Single(service.Requests);
        Assert.Equal(cts.Token, service.Tokens.Single());
        Assert.Equal(action.Uri, service.Requests[0].GetUri());
        Assert.Equal(action.Method, service.Requests[0].Method);
        Assert.Equal("yes", service.Requests[0].Headers.Get("X-Modified"));
        Assert.Equal(1, action.ModifyRequestCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenResponseHasException_ReturnsExceptionWithoutCallingGetResult()
    {
        var exception = new InvalidOperationException("send failed");
        var response = CreateResponse(exception: exception);
        var service = new CaptureHttpService(response);
        var client = new TestUserClient(ServiceProvider.GetRequiredService<ILoggerFactory>())
        {
            HttpService = service,
        };
        var action = new ServiceBackedUserClientHttpAction(client);

        var result = await ((FclEx.Actions.IAction<string>)action).ExecuteAsync();

        Assert.True(result.IsError);
        Assert.Same(exception, result.Exception);
        Assert.Equal(0, action.GetResultCallCount);
    }

    [Fact]
    public async Task HandleResponseAsync_WhenEnsureSuccessStatusCodeIsTrue_ReturnsErrorForFailureStatus()
    {
        var client = new TestUserClient(ServiceProvider.GetRequiredService<ILoggerFactory>());
        var action = new InspectableUserClientHttpAction(client);
        var response = CreateResponse("missing", HttpStatusCode.NotFound);

        var result = await action.HandleResponseAsync(response);

        Assert.True(result.IsError);
        Assert.Contains("NotFound", result.Exception.Message);
    }

    private static FclEx.Http.HttpResponse CreateResponse(
        string responseString = "",
        HttpStatusCode statusCode = HttpStatusCode.OK,
        Exception? exception = null)
    {
        var request = FclEx.Http.HttpRequest.Get("https://example.com/source");
        var response = exception is null
            ? new FclEx.Http.HttpResponse(request)
            : FclEx.Http.HttpResponse.FromError(request, exception);
        typeof(FclEx.Http.HttpResponse)
            .GetProperty(nameof(FclEx.Http.HttpResponse.ResponseString))!
            .SetValue(response, responseString);
        typeof(FclEx.Http.HttpResponse)
            .GetProperty(nameof(FclEx.Http.HttpResponse.StatusCode))!
            .SetValue(response, statusCode);
        return response;
    }

    private sealed class CaptureHttpService(FclEx.Http.HttpResponse response) : IHttpService
    {
        public List<FclEx.Http.HttpRequest> Requests { get; } = [];

        public List<CancellationToken> Tokens { get; } = [];

        public Task<FclEx.Http.HttpResponse> SendAsync(FclEx.Http.HttpRequest request, CancellationToken token = default)
        {
            Requests.Add(request);
            Tokens.Add(token);
            return Task.FromResult(response);
        }

        public void AddCookie(Cookie cookie, Uri? uri = null, bool overrideDomain = false) { }

        public Cookie? GetCookie(Uri uri, string name) => null;

        public IReadOnlyCollection<Cookie> GetCookies(Uri uri) => [];

        public IReadOnlyCollection<Cookie> GetAllCookies() => [];

        public IWebProxy? Proxy { get; set; }

        public ILogger Logger { get; set; } = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

        public void Dispose() { }
    }

    private sealed class ServiceBackedUserClientHttpAction(TestUserClient client)
        : UserClientHttpAction<TestUserClient, string>(client)
    {
        public int ModifyRequestCallCount { get; private set; }

        public int GetResultCallCount { get; private set; }

        public override Uri Uri { get; } = new("https://example.com/api");

        public override HttpMethod Method { get; } = HttpMethod.Put;

        public override void ModifyRequest(FclEx.Http.HttpRequest request)
        {
            ModifyRequestCallCount++;
            request.SetHeader("X-Modified", "yes");
        }

        public override OperationResult<string> GetResult(FclEx.Http.HttpResponse response)
        {
            GetResultCallCount++;
            return Operation.Success(response.ResponseString);
        }
    }

    private sealed class NonEnforcingUserClientHttpAction(TestUserClient client)
        : UserClientHttpAction<TestUserClient, string>(client)
    {
        public override Uri Uri { get; } = new("https://example.com/missing");

        public override HttpMethod Method { get; } = HttpMethod.Get;

        public override bool EnsureSuccessStatusCode => false;

        public override OperationResult<string> GetResult(FclEx.Http.HttpResponse response)
        {
            return Operation.Success("accepted");
        }

        public override Task<FclEx.Http.HttpResponse> GetResponseAsync(FclEx.Http.HttpRequest request, CancellationToken token = default)
        {
            var response = new FclEx.Http.HttpResponse(request);
            typeof(FclEx.Http.HttpResponse)
                .GetProperty(nameof(FclEx.Http.HttpResponse.StatusCode))!
                .SetValue(response, HttpStatusCode.NotFound);
            return Task.FromResult(response);
        }
    }

    private sealed class InspectableUserClientHttpAction(TestUserClient client)
        : UserClientHttpAction<TestUserClient, string>(client)
    {
        public override Uri Uri { get; } = new("https://example.com/api");

        public override HttpMethod Method { get; } = HttpMethod.Post;

        public override OperationResult<string> GetResult(FclEx.Http.HttpResponse response)
        {
            return Operation.Success("ok");
        }
    }
}
