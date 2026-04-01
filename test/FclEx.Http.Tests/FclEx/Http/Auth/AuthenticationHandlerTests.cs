namespace FclEx.Http.Auth;

public class AuthenticationHandlerTests : AuthTests
{
    public static HttpClient CreateHttpClient(string[] scopes, bool requireToken = true, MutateTokenResponseHandler? handler = null)
    {
        var provider = new ServiceCollection()
            .AddTestTokenProvider()
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
        var client = CreateHttpClient(["test-scope"], requireToken: false);
        var request = new HttpRequestMessage(HttpMethod.Get, TestUri.WithPath("/auth/test"));
        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task WithToken_200()
    {
        var client = CreateHttpClient(["test-scope"]);
        var request = new HttpRequestMessage(HttpMethod.Get, TestUri.WithPath("/auth/test"));
        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
