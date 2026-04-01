using static Duende.IdentityModel.OidcConstants;

namespace FclEx.Http.Auth;

public class ClientCredentialsTokenProviderTests : AuthTests
{
    [Fact]
    public async Task GetToken_ShouldReturnAccessToken()
    {
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
        var handler = new MutateTokenResponseHandler();
        var provider = CreateTestTokenProvider(handler);
        await provider.GetTokenAsync("scope1");
        await provider.GetTokenAsync("scope2");

        Assert.Equal(2, handler.TokenRequestCount);
    }

    [Fact]
    public async Task GetToken_ShouldRefresh_WhenExpired()
    {
        var handler = new MutateTokenResponseHandler((_, _, _, m) => m[TokenResponse.ExpiresIn] = 1);
        var provider = CreateTestTokenProvider(handler);
        await provider.GetTokenAsync("api");
        await Task.Delay(1500);
        await provider.GetTokenAsync("api");

        Assert.Equal(2, handler.TokenRequestCount);
    }

    [Fact]
    public async Task GetToken_ShouldOnlyRequestOnce_UnderConcurrency()
    {
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
        var handler = new MutateTokenResponseHandler();
        var provider = CreateTestTokenProvider(handler);
        await provider.GetTokenAsync("api");
        await provider.GetTokenAsync("api", forceRefresh: true);

        Assert.Equal(2, handler.TokenRequestCount);
    }
}
