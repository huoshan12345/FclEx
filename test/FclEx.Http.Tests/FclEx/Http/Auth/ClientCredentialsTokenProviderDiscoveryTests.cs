namespace FclEx.Http.Auth;

public class ClientCredentialsTokenProviderDiscoveryTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetTokenAsync_WhenDiscoveryAdvertisesJsonWebKeySet_ReturnsAccessToken(bool requireKeySet)
    {
        using var handler = new DiscoveryWithJsonWebKeySetHandler();
        using var httpClient = new HttpClient(handler);
        var provider = new ClientCredentialsTokenProvider(new()
        {
            Authority = "https://auth.example.com",
            ClientId = "client",
            ClientSecret = "secret",
            Policy = new()
            {
                RequireKeySet = requireKeySet,
            },
        }, httpClient);

        // An advertised JWKS is fetched even when RequireKeySet is false, as with Keycloak discovery.
        // This exercises Duende's generated JSON metadata with the System.Text.Json version resolved by FclEx.
        var token = await provider.GetTokenAsync(["api"], cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("access-token", token);
        Assert.Equal(
            ["/.well-known/openid-configuration", "/connect/certs", "/connect/token"],
            handler.RequestPaths);
    }

    private sealed class DiscoveryWithJsonWebKeySetHandler : HttpMessageHandler
    {
        public List<string> RequestPaths { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = request.RequestUri!.AbsolutePath;
            RequestPaths.Add(path);

            var content = path switch
            {
                "/.well-known/openid-configuration" => """
                    {
                      "issuer": "https://auth.example.com",
                      "token_endpoint": "https://auth.example.com/connect/token",
                      "jwks_uri": "https://auth.example.com/connect/certs"
                    }
                    """,
                "/connect/certs" => """
                    {
                      "keys": [
                        { "kty": "RSA", "kid": "test-key", "use": "sig", "e": "AQAB", "n": "AQAB" }
                      ]
                    }
                    """,
                "/connect/token" => """
                    {
                      "access_token": "access-token",
                      "expires_in": 3600,
                      "token_type": "Bearer"
                    }
                    """,
                _ => throw new InvalidOperationException($"Unexpected request: {request.Method} {request.RequestUri}"),
            };

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/json"),
                RequestMessage = request,
            });
        }
    }
}
