namespace FclEx.Http.Auth;

public class AuthenticationHandlerRegistrationTests
{
    [Fact]
    public async Task AddAuthenticationHandler_WithOptionsCallback_UsesConfiguredOptions()
    {
        var tokenProvider = new TestAccessTokenProvider("token");
        var innerHandler = new CaptureAuthorizationHandler();
        var builder = CreateBuilder(tokenProvider, innerHandler);
        builder.AddAuthenticationHandler((Action<AuthenticationHandlerOptions>)(options =>
        {
            options.TokenProviderName = "named";
            options.Scopes = ["api.read"];
        }));

        await AssertRequestUsesToken(builder.Services, tokenProvider, innerHandler, "api.read");
    }

    [Fact]
    public async Task AddAuthenticationHandler_WithServiceProviderCallback_ResolvesConfigurationDependency()
    {
        var tokenProvider = new TestAccessTokenProvider("token");
        var innerHandler = new CaptureAuthorizationHandler();
        var settings = new AuthenticationSettings("named", "api.write");
        var builder = CreateBuilder(tokenProvider, innerHandler);
        builder.Services.AddSingleton(settings);
        builder.AddAuthenticationHandler((options, provider) =>
            {
                var resolvedSettings = provider.GetRequiredService<AuthenticationSettings>();
                options.TokenProviderName = resolvedSettings.TokenProviderName;
                options.Scopes = [resolvedSettings.Scope];
            });

        await AssertRequestUsesToken(builder.Services, tokenProvider, innerHandler, settings.Scope);
    }

    [Fact]
    public async Task AddAuthenticationHandlerBy_WithOptionsFactory_UsesDependencyConfiguration()
    {
        var tokenProvider = new TestAccessTokenProvider("token");
        var innerHandler = new CaptureAuthorizationHandler();
        var settings = new AuthenticationSettings("named", "api.delete");
        var builder = CreateBuilder(tokenProvider, innerHandler);
        builder.Services.AddSingleton(settings);
        builder.AddAuthenticationHandlerBy<AuthenticationSettings>(resolvedSettings => new()
            {
                TokenProviderName = resolvedSettings.TokenProviderName,
                Scopes = [resolvedSettings.Scope],
            });

        await AssertRequestUsesToken(builder.Services, tokenProvider, innerHandler, settings.Scope);
    }

    [Fact]
    public async Task AddAuthenticationHandlerBy_WithOptionsAction_UsesDependencyConfiguration()
    {
        var tokenProvider = new TestAccessTokenProvider("token");
        var innerHandler = new CaptureAuthorizationHandler();
        var settings = new AuthenticationSettings("named", "api.update");
        var builder = CreateBuilder(tokenProvider, innerHandler);
        builder.Services.AddSingleton(settings);
        builder.AddAuthenticationHandlerBy<AuthenticationSettings>((options, resolvedSettings) =>
            {
                options.TokenProviderName = resolvedSettings.TokenProviderName;
                options.Scopes = [resolvedSettings.Scope];
            });

        await AssertRequestUsesToken(builder.Services, tokenProvider, innerHandler, settings.Scope);
    }

    private static IHttpClientBuilder CreateBuilder(TestAccessTokenProvider tokenProvider, CaptureAuthorizationHandler innerHandler)
    {
        return new ServiceCollection()
            .AddAccessTokenProvider("named", _ => tokenProvider)
            .AddHttpClient("api")
            .ConfigurePrimaryHttpMessageHandler(() => innerHandler);
    }

    private static async Task AssertRequestUsesToken(
        IServiceCollection services,
        TestAccessTokenProvider tokenProvider,
        CaptureAuthorizationHandler innerHandler,
        string expectedScope)
    {
        using var serviceProvider = services.BuildServiceProvider();
        using var client = serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("api");
        using var response = await client.GetAsync("https://example.com/resource");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Bearer", innerHandler.AuthorizationHeader?.Scheme);
        Assert.Equal("token", innerHandler.AuthorizationHeader?.Parameter);
        Assert.Equal(new[] { expectedScope }, Assert.Single(tokenProvider.RequestedScopes));
    }

    private sealed record AuthenticationSettings(string TokenProviderName, string Scope);

    private sealed class TestAccessTokenProvider(string token) : IAccessTokenProvider
    {
        public List<string[]> RequestedScopes { get; } = [];

        public Task<string> GetTokenAsync(string[] scopes, bool forceRefresh = false, CancellationToken cancellationToken = default)
        {
            RequestedScopes.Add(scopes);
            return Task.FromResult(token);
        }
    }

    private sealed class CaptureAuthorizationHandler : HttpMessageHandler
    {
        public AuthenticationHeaderValue? AuthorizationHeader { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            AuthorizationHeader = request.Headers.Authorization;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
