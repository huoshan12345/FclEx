using System.Collections.Concurrent;
using Duende.IdentityModel.Client;

namespace FclEx.Http.Auth;

public class ClientCredentialsTokenProviderTests : AuthTests
{
    [Fact]
    public void Options_DefaultsToEmptyCredentialsAndDiscoveryPolicyWithoutKeySetRequirement()
    {
        var options = new ClientCredentialsTokenProviderOptions();

        Assert.Equal("", options.Authority);
        Assert.Equal("", options.ClientId);
        Assert.Equal("", options.ClientSecret);
        Assert.NotNull(options.Policy);
        Assert.False(options.Policy.RequireKeySet);
    }

    [Fact]
    public void Constructor_UsesConfiguredDiscoveryPolicy()
    {
        var policy = new DiscoveryPolicy
        {
            RequireHttps = false,
            ValidateEndpoints = false,
            RequireKeySet = false,
        };
        var provider = new ClientCredentialsTokenProvider(new()
        {
            Authority = "http://localhost/oauth",
            ClientId = "client",
            ClientSecret = "secret",
            Policy = policy,
        }, () => throw new InvalidOperationException("No HTTP request should be sent in this test."));

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
        var provider = new ClientCredentialsTokenProvider(new()
        {
            Authority = "https://auth.example.com",
            ClientId = "client",
            ClientSecret = "secret",
            Policy = new()
            {
                RequireKeySet = false,
            },
        }, httpClient);

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
        var provider = new ClientCredentialsTokenProvider(new()
        {
            Authority = "https://auth.example.com",
            ClientId = "client",
            ClientSecret = "secret",
            Policy = new()
            {
                RequireKeySet = false,
            },
        }, httpClient);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            provider.GetTokenAsync("api", cancellationToken: cts.Token));

        Assert.Equal(cancelRequestIndex, handler.RequestCount);
    }

    [Fact]
    public async Task GetToken_WhenFuncCreatesClients_DisposesCreatedClientsByDefault()
    {
        var clients = new List<TrackingHttpClient>();
        var provider = new ClientCredentialsTokenProvider(new()
        {
            Authority = "https://auth.example.com",
            ClientId = "client",
            ClientSecret = "secret",
            Policy = new()
            {
                RequireKeySet = false,
            },
        }, () =>
        {
            var client = new TrackingHttpClient(new TokenProviderHandler());
            clients.Add(client);
            return client;
        });

        var token = await provider.GetTokenAsync("api");

        Assert.Equal("access-token", token);
        Assert.Equal(2, clients.Count);
        Assert.All(clients, client => Assert.True(client.IsDisposed));
    }

    [Fact]
    public async Task GetToken_WhenUsingHttpClientFactory_DoesNotDisposeFactoryClient()
    {
        using var client = new TrackingHttpClient(new TokenProviderHandler());
        var provider = new ClientCredentialsTokenProvider(new()
        {
            Authority = "https://auth.example.com",
            ClientId = "client",
            ClientSecret = "secret",
            Policy = new()
            {
                RequireKeySet = false,
            },
        }, new StaticHttpClientFactory(client));

        var token = await provider.GetTokenAsync("api");

        Assert.Equal("access-token", token);
        Assert.False(client.IsDisposed);
    }

    [Fact]
    public async Task GetToken_WhenDiscoveryIsSharedByConcurrentScopes_DoesNotCreateUnusedDiscoveryClients()
    {
        var clients = new ConcurrentBag<TrackingHttpClient>();
        var handler = new DelayedTokenProviderHandler();
        var provider = new ClientCredentialsTokenProvider(new()
        {
            Authority = "https://auth.example.com",
            ClientId = "client",
            ClientSecret = "secret",
            Policy = new()
            {
                RequireKeySet = false,
            },
        }, () =>
        {
            var client = new TrackingHttpClient(handler);
            clients.Add(client);
            return client;
        });

        var tokens = await Task.WhenAll(
            provider.GetTokenAsync("scope1"),
            provider.GetTokenAsync("scope2"));

        Assert.Equal(["access-token", "access-token"], tokens);
        Assert.Equal(1, handler.DiscoveryRequestCount);
        Assert.Equal(2, handler.TokenRequestCount);
        Assert.Equal(3, clients.Count);
        Assert.All(clients, client => Assert.True(client.IsDisposed));
    }

    [Fact]
    public async Task GetToken_WhenMultipleScopesAreProvided_SendsSpaceSeparatedScope()
    {
        var handler = new CaptureTokenRequestHandler();
        using var httpClient = new HttpClient(handler);
        var provider = CreateLocalProvider(httpClient);

        var token = await provider.GetTokenAsync(["scope-a", "scope-b"]);

        Assert.Equal("client-token-1", token);
        var request = Assert.Single(handler.TokenRequests);
        Assert.Equal("client_credentials", request["grant_type"]);
        Assert.Equal("scope-a scope-b", request["scope"]);
    }

    [Fact]
    public async Task GetToken_WhenExpiredCachedTokenHasRefreshToken_UsesRefreshTokenBeforeClientCredentials()
    {
        var handler = new CaptureTokenRequestHandler
        {
            ClientCredentialsExpiresIn = 1,
        };
        using var httpClient = new HttpClient(handler);
        var provider = CreateLocalProvider(httpClient);

        var first = await provider.GetTokenAsync("api");
        var second = await provider.GetTokenAsync("api");

        Assert.Equal("client-token-1", first);
        Assert.Equal("refresh-token-1", second);
        Assert.Equal(2, handler.TokenRequests.Count);
        Assert.Equal("client_credentials", handler.TokenRequests[0]["grant_type"]);
        Assert.Equal("refresh_token", handler.TokenRequests[1]["grant_type"]);
        Assert.Equal("refresh-1", handler.TokenRequests[1]["refresh_token"]);
    }

    [Fact]
    public async Task GetToken_WhenRefreshTokenRequestFails_FallsBackToClientCredentials()
    {
        var handler = new CaptureTokenRequestHandler
        {
            ClientCredentialsExpiresIn = 1,
            RefreshFails = true,
        };
        using var httpClient = new HttpClient(handler);
        var provider = CreateLocalProvider(httpClient);

        var first = await provider.GetTokenAsync("api");
        var second = await provider.GetTokenAsync("api");

        Assert.Equal("client-token-1", first);
        Assert.Equal("client-token-2", second);
        Assert.Equal(3, handler.TokenRequests.Count);
        Assert.Equal("client_credentials", handler.TokenRequests[0]["grant_type"]);
        Assert.Equal("refresh_token", handler.TokenRequests[1]["grant_type"]);
        Assert.Equal("client_credentials", handler.TokenRequests[2]["grant_type"]);
    }

    private static ClientCredentialsTokenProvider CreateLocalProvider(HttpClient httpClient)
    {
        return new(new()
        {
            Authority = "https://auth.example.com",
            ClientId = "client",
            ClientSecret = "secret",
            Policy = new()
            {
                RequireKeySet = false,
            },
        }, httpClient);
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

    private sealed class TokenProviderHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
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

    private sealed class CaptureTokenRequestHandler : HttpMessageHandler
    {
        private int _clientTokenIndex;
        private int _refreshTokenIndex;

        public int ClientCredentialsExpiresIn { get; init; } = 3600;

        public bool RefreshFails { get; init; }

        public List<Dictionary<string, string>> TokenRequests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("/.well-known/openid-configuration", StringComparison.Ordinal))
            {
                return JsonResponse("""
                                    {
                                      "issuer": "https://auth.example.com",
                                      "token_endpoint": "https://auth.example.com/connect/token"
                                    }
                                    """, request);
            }

            var form = await request.Content!.ReadAsStringAsync(cancellationToken);
            var captured = ParseForm(form);
            TokenRequests.Add(captured);

            return captured["grant_type"] == "refresh_token"
                ? CreateRefreshTokenResponse(request)
                : CreateClientCredentialsResponse(request);
        }

        private static Dictionary<string, string> ParseForm(string form)
        {
            return form
                .Split(['&'], StringSplitOptions.RemoveEmptyEntries)
                .Select(SplitPair)
                .ToDictionary(
                    pair => WebUtility.UrlDecode(pair.Key),
                    pair => WebUtility.UrlDecode(pair.Value));
        }

        private static KeyValuePair<string, string> SplitPair(string pair)
        {
            var index = pair.IndexOf('=');
            return index < 0
                ? KeyValuePair.Create(pair, "")
                : KeyValuePair.Create(pair[..index], pair[(index + 1)..]);
        }

        private HttpResponseMessage CreateClientCredentialsResponse(HttpRequestMessage request)
        {
            _clientTokenIndex++;
            return JsonResponse($$"""
                                  {
                                    "access_token": "client-token-{{_clientTokenIndex}}",
                                    "refresh_token": "refresh-{{_clientTokenIndex}}",
                                    "expires_in": {{ClientCredentialsExpiresIn}},
                                    "token_type": "Bearer"
                                  }
                                  """, request);
        }

        private HttpResponseMessage CreateRefreshTokenResponse(HttpRequestMessage request)
        {
            if (RefreshFails)
            {
                return JsonResponse("""
                                    {
                                      "error": "invalid_grant",
                                      "error_description": "refresh failed"
                                    }
                                    """, request, HttpStatusCode.BadRequest);
            }

            _refreshTokenIndex++;
            return JsonResponse($$"""
                                  {
                                    "access_token": "refresh-token-{{_refreshTokenIndex}}",
                                    "refresh_token": "refresh-next-{{_refreshTokenIndex}}",
                                    "expires_in": 3600,
                                    "token_type": "Bearer"
                                  }
                                  """, request);
        }

        private static HttpResponseMessage JsonResponse(
            string json,
            HttpRequestMessage request,
            HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            return new(statusCode)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
                RequestMessage = request,
            };
        }
    }

    private sealed class DelayedTokenProviderHandler : HttpMessageHandler
    {
        private int _discoveryRequestCount;

        private int _tokenRequestCount;

        public int DiscoveryRequestCount => _discoveryRequestCount;

        public int TokenRequestCount => _tokenRequestCount;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("/.well-known/openid-configuration", StringComparison.Ordinal))
            {
                Interlocked.Increment(ref _discoveryRequestCount);
                await Task.Delay(50, cancellationToken);
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

            Interlocked.Increment(ref _tokenRequestCount);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                                            {
                                              "access_token": "access-token",
                                              "expires_in": 3600,
                                              "token_type": "Bearer"
                                            }
                                            """, Encoding.UTF8, "application/json"),
                RequestMessage = request,
            };
        }
    }

    private sealed class TrackingHttpClient(HttpMessageHandler handler) : HttpClient(handler)
    {
        public bool IsDisposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }
    }

    private sealed class StaticHttpClientFactory(HttpClient client) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            return client;
        }
    }
}
