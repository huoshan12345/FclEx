namespace FclEx.Web;

public class UserClientHttpActionTests : WebTests
{
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
}
