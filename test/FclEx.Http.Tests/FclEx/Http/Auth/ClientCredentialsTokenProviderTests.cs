using Duende.IdentityModel.Client;

namespace FclEx.Http.Auth;

public class ClientCredentialsTokenProviderTests : AuthTests
{
    [Fact]
    public void Constructor_UsesConfiguredDiscoveryPolicy()
    {
        var policy = new DiscoveryPolicy
        {
            RequireHttps = false,
            ValidateEndpoints = false,
            RequireKeySet = false,
        };
        var provider = new ClientCredentialsTokenProvider(
            () => throw new InvalidOperationException("No HTTP request should be sent in this test."),
            new()
            {
                Authority = "http://localhost/oauth",
                ClientId = "client",
                ClientSecret = "secret",
                Policy = policy,
            });

        var request = typeof(ClientCredentialsTokenProvider)
            .GetRequiredField("_documentRequest")
            .GetRequiredValue<DiscoveryDocumentRequest>(provider);

        Assert.Same(policy, request.Policy);
    }

    [Fact]
    public async Task GetToken_ShouldReturnAccessToken()
    {
        if (HasApiServer == false)
            return;

        const string scope = "api";
        var provider = CreateTestTokenProvider();
        var token = await provider.GetTokenAsync(scope);
        var jwt = new JsonWebToken(token);
        var scopes = jwt.GetScopes();
        Assert.Single(scopes);
        Assert.Equal(scope, scopes[0]);
    }

    [Fact]
    public async Task GetToken_ShouldUseCache_ForSameScope()
    {
        if (HasApiServer == false)
            return;

        var handler = new MutateTokenResponseHandler();
        var provider = CreateTestTokenProvider(handler);
        var token1 = await provider.GetTokenAsync("api");
        var token2 = await provider.GetTokenAsync("api");

        Assert.Equal(token1, token2);
        Assert.Equal(1, handler.TokenRequestCount);
    }

    [Fact]
    public async Task GetToken_ShouldCachePerScope()
    {
        if (HasApiServer == false)
            return;

        var handler = new MutateTokenResponseHandler();
        var provider = CreateTestTokenProvider(handler);
        await provider.GetTokenAsync("scope1");
        await provider.GetTokenAsync("scope2");

        Assert.Equal(2, handler.TokenRequestCount);
    }

    [Fact]
    public async Task GetToken_ShouldRefresh_WhenExpired()
    {
        if (HasApiServer == false)
            return;

        var handler = new MutateTokenResponseHandler((_, _, _, m) => m[OidcConstants.TokenResponse.ExpiresIn] = 1);
        var provider = CreateTestTokenProvider(handler);
        await provider.GetTokenAsync("api");
        await Task.Delay(1500);
        await provider.GetTokenAsync("api");

        Assert.Equal(2, handler.TokenRequestCount);
    }

    [Fact]
    public async Task GetToken_ShouldOnlyRequestOnce_UnderConcurrency()
    {
        if (HasApiServer == false)
            return;

        const string scope = "api";
        var handler = new MutateTokenResponseHandler();
        var provider = CreateTestTokenProvider(handler);
        var tasks = Enumerable.Range(0, 20)
            .Select(_ => provider.GetTokenAsync(scope));
        var tokens = await Task.WhenAll(tasks);

        Assert.All(tokens, t =>
        {
            var jwt = new JsonWebToken(t);
            var scopes = jwt.GetScopes();
            Assert.Single(scopes);
            Assert.Equal(scope, scopes[0]);
        });
        Assert.Equal(1, handler.TokenRequestCount);
    }

    [Fact]
    public async Task GetToken_ShouldForceRefresh_WhenRequested()
    {
        if (HasApiServer == false)
            return;

        var handler = new MutateTokenResponseHandler();
        var provider = CreateTestTokenProvider(handler);
        await provider.GetTokenAsync("api");
        await provider.GetTokenAsync("api", forceRefresh: true);

        Assert.Equal(2, handler.TokenRequestCount);
    }

    [Fact]
    public async Task GetToken_ShouldSendCancelableTokens_ToDiscoveryAndTokenRequests()
    {
        var handler = new CaptureCancellationTokenHandler();
        using var httpClient = new HttpClient(handler);
        var provider = new ClientCredentialsTokenProvider(
            // ReSharper disable once AccessToDisposedClosure
            () => httpClient,
            new()
            {
                Authority = "https://auth.example.com",
                ClientId = "client",
                ClientSecret = "secret",
                Policy = new()
                {
                    RequireKeySet = false,
                },
            });

        using var cts = new CancellationTokenSource();

        var token = await provider.GetTokenAsync("api", cancellationToken: cts.Token);

        Assert.Equal("access-token", token);
        Assert.Equal(2, handler.CancellationTokens.Count);
        Assert.All(handler.CancellationTokens, t => Assert.True(t.CanBeCanceled));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task GetToken_WhenCancellationIsRequestedDuringHttpRequest_CancelsRequest(int cancelRequestIndex)
    {
        using var cts = new CancellationTokenSource();
        var handler = new CancelRequestHandler(cts, cancelRequestIndex);
        using var httpClient = new HttpClient(handler);
        var provider = new ClientCredentialsTokenProvider(
            // ReSharper disable once AccessToDisposedClosure
            () => httpClient,
            new()
            {
                Authority = "https://auth.example.com",
                ClientId = "client",
                ClientSecret = "secret",
                Policy = new()
                {
                    RequireKeySet = false,
                },
            });

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            provider.GetTokenAsync("api", cancellationToken: cts.Token));

        Assert.Equal(cancelRequestIndex, handler.RequestCount);
    }

    private sealed class CaptureCancellationTokenHandler : HttpMessageHandler
    {
        public List<CancellationToken> CancellationTokens { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CancellationTokens.Add(cancellationToken);

            var content = request.RequestUri!.AbsolutePath.EndsWith("/.well-known/openid-configuration", StringComparison.Ordinal)
                ? """
                  {
                    "issuer": "https://auth.example.com",
                    "token_endpoint": "https://auth.example.com/connect/token"
                  }
                  """
                : """
                  {
                    "access_token": "access-token",
                    "expires_in": 3600,
                    "token_type": "Bearer"
                  }
                  """;

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/json"),
                RequestMessage = request,
            });
        }
    }

    private sealed class CancelRequestHandler(CancellationTokenSource source, int cancelRequestIndex) : HttpMessageHandler
    {
        public int RequestCount { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestCount++;
            if (RequestCount == cancelRequestIndex)
            {
                source.Cancel();
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                throw new InvalidOperationException("Cancellation was not propagated to the HTTP request.");
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                                            {
                                              "issuer": "https://auth.example.com",
                                              "token_endpoint": "https://auth.example.com/connect/token"
                                            }
                                            """, Encoding.UTF8, "application/json"),
                RequestMessage = request,
            };
        }
    }
}
