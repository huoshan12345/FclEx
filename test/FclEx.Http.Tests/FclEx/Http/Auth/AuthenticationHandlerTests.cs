using static Duende.IdentityModel.OidcConstants;

namespace FclEx.Http.Auth;

public class AuthenticationHandlerTests : AuthTests
{
    public static HttpClient CreateHttpClient(string[] scopes, MutateTokenResponseHandler? handler = null, bool requireToken = true)
    {
        var provider = new ServiceCollection()
            .AddTestTokenProvider(handler)
            .AddHttpClient(string.Empty)
            .AddHttpMessageHandlerBy<AuthenticationHandler, IAccessTokenProvider>(m => new AuthenticationHandler(m, scopes, requireToken))
            .Services
            .BuildServiceProvider();

        var factory = provider.GetRequiredService<IHttpClientFactory>();
        return factory.CreateClient();
    }

    [Fact]
    public async Task WithoutToken_401()
    {
        var client = CreateHttpClient([RequiredScope], requireToken: false);
        var request = new HttpRequestMessage(HttpMethod.Get, TestUri.WithPath("/auth/test"));
        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task WithToken_200()
    {
        var client = CreateHttpClient([RequiredScope]);
        var request = new HttpRequestMessage(HttpMethod.Get, TestUri.WithPath("/auth/test"));
        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task WithWrongScope_403()
    {
        var client = CreateHttpClient([RequiredScope + "-1"]);
        var request = new HttpRequestMessage(HttpMethod.Get, TestUri.WithPath("/auth/test"));
        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_UseCachedToken_AcrossMultipleRequests()
    {
        var handler = new MutateTokenResponseHandler();
        var client = CreateHttpClient([RequiredScope], handler);

        for (var i = 0; i < 3; i++)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, TestUri.WithPath("/auth/test"));
            var response = await client.SendAsync(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        Assert.Equal(1, handler.TokenRequestCount);
    }

    [Fact]
    public async Task Should_Retry_On401_WithNewToken()
    {
        var handler = new MutateTokenResponseHandler((h, req, res, json) =>
        {
            if (h.TokenRequestCount == 1)
            {
                json[TokenResponse.AccessToken] = "";
            }
        });

        var client = CreateHttpClient([RequiredScope], handler);
        var request = new HttpRequestMessage(HttpMethod.Get, TestUri.WithPath("/auth/test"));
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, handler.TokenRequestCount);
    }

    [Fact]
    public async Task Should_NotRetry_IfSecondAttemptFails()
    {
        var handler = new MutateTokenResponseHandler((h, req, res, json) =>
        {
            json[TokenResponse.AccessToken] = "";
        });

        var client = CreateHttpClient([RequiredScope], handler);
        var request = new HttpRequestMessage(HttpMethod.Get, TestUri.WithPath("/auth/test"));
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(2, handler.TokenRequestCount);
    }
}
