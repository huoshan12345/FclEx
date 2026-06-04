namespace FclEx.Http.Auth;

public class AuthenticationHandlerTests : AuthTests
{
    public static HttpClient CreateHttpClient(string[] scopes, MutateTokenResponseHandler? handler = null, bool requireToken = true)
    {
        var provider = new ServiceCollection()
            .AddTestTokenProvider(handler)
            .AddHttpClient(string.Empty)
            .AddHttpMessageHandlerBy<IAccessTokenProvider>(m => new AuthenticationHandler(m, scopes, requireToken))
            .Services
            .BuildServiceProvider();

        var factory = provider.GetRequiredService<IHttpClientFactory>();
        return factory.CreateClient();
    }

    [Fact]
    public async Task WithoutToken_401()
    {
        if (HasApiServer == false)
            return;

        var client = CreateHttpClient([RequiredScope], requireToken: false);
        var request = new HttpRequestMessage(HttpMethod.Get, TestUri.WithPath(TestApiPaths.AuthTest));
        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task WithToken_200()
    {
        if (HasApiServer == false)
            return;

        var client = CreateHttpClient([RequiredScope]);
        var request = new HttpRequestMessage(HttpMethod.Get, TestUri.WithPath(TestApiPaths.AuthTest));
        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task WithWrongScope_403()
    {
        if (HasApiServer == false)
            return;

        var client = CreateHttpClient([RequiredScope + "-1"]);
        var request = new HttpRequestMessage(HttpMethod.Get, TestUri.WithPath(TestApiPaths.AuthTest));
        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_UseCachedToken_AcrossMultipleRequests()
    {
        if (HasApiServer == false)
            return;

        var handler = new MutateTokenResponseHandler();
        var client = CreateHttpClient([RequiredScope], handler);

        for (var i = 0; i < 3; i++)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, TestUri.WithPath(TestApiPaths.AuthTest));
            var response = await client.SendAsync(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        Assert.Equal(1, handler.TokenRequestCount);
    }

    [Fact]
    public async Task Should_Retry_On401_WithNewToken()
    {
        if (HasApiServer == false)
            return;

        var handler = new MutateTokenResponseHandler((h, req, res, json) =>
        {
            if (h.TokenRequestCount == 1)
            {
                json[TokenResponse.AccessToken] = "";
            }
        });

        var client = CreateHttpClient([RequiredScope], handler);
        var request = new HttpRequestMessage(HttpMethod.Get, TestUri.WithPath(TestApiPaths.AuthTest));
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, handler.TokenRequestCount);
    }

    [Fact]
    public async Task Should_NotRetry_IfSecondAttemptFails()
    {
        if (HasApiServer == false)
            return;

        var handler = new MutateTokenResponseHandler((h, req, res, json) =>
        {
            json[TokenResponse.AccessToken] = "";
        });

        var client = CreateHttpClient([RequiredScope], handler);
        var request = new HttpRequestMessage(HttpMethod.Get, TestUri.WithPath(TestApiPaths.AuthTest));
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(2, handler.TokenRequestCount);
    }

    [Fact]
    public async Task Should_ReuseRequestMessage_WhenRetryingInsideDelegatingHandler()
    {
        var tokenProvider = new TestAccessTokenProvider("expired-token", "fresh-token");
        var innerHandler = new UnauthorizedThenOkHandler();
        using var handler = new AuthenticationHandler(tokenProvider, [RequiredScope])
        {
            InnerHandler = innerHandler,
        };
        using var invoker = new HttpMessageInvoker(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/api");

        using var response = await invoker.SendAsync(request, CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, tokenProvider.Requests.Count);
        Assert.Equal(new[] { RequiredScope }, tokenProvider.Requests[0].Scopes);
        Assert.Equal(new[] { RequiredScope }, tokenProvider.Requests[1].Scopes);
        Assert.False(tokenProvider.Requests[0].ForceRefresh);
        Assert.True(tokenProvider.Requests[1].ForceRefresh);
        Assert.Equal(2, innerHandler.Requests.Count);
        Assert.Same(request, innerHandler.Requests[0]);
        Assert.Same(request, innerHandler.Requests[1]);
        Assert.Equal(new[] { "expired-token", "fresh-token" }, innerHandler.AuthorizationTokens);
    }

    private sealed class TestAccessTokenProvider(params string[] tokens) : IAccessTokenProvider
    {
        private int _index;

        public List<(string[] Scopes, bool ForceRefresh)> Requests { get; } = [];

        public Task<string> GetTokenAsync(string[] scopes, bool forceRefresh = false, CancellationToken cancellationToken = default)
        {
            Requests.Add((scopes, forceRefresh));
            var token = tokens[Math.Min(_index, tokens.Length - 1)];
            _index++;
            return Task.FromResult(token);
        }
    }

    private sealed class UnauthorizedThenOkHandler : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = [];
        public List<string?> AuthorizationTokens { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            AuthorizationTokens.Add(request.Headers.Authorization?.Parameter);

            var statusCode = Requests.Count == 1
                ? HttpStatusCode.Unauthorized
                : HttpStatusCode.OK;

            return Task.FromResult(new HttpResponseMessage(statusCode));
        }
    }
}
