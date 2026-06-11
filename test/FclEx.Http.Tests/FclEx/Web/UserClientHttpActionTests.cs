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
