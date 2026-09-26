namespace FclEx.Http.Auth;

public class AccessTokenProviderRegistrationTests
{
    [Fact]
    public void AddAccessTokenProvider_WithoutName_RegistersDefaultProvider()
    {
        var expected = new TestAccessTokenProvider("default");
        var services = new ServiceCollection()
            .AddAccessTokenProvider(_ => expected);

        using var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IAccessTokenProviderFactory>();

        Assert.Same(expected, factory.GetRequired(string.Empty));
        Assert.Same(expected, factory.GetRequired(string.Empty));
    }

    [Fact]
    public void AddAccessTokenProvider_WithDifferentNames_RegistersIndependentProviders()
    {
        var first = new TestAccessTokenProvider("first");
        var second = new TestAccessTokenProvider("second");
        var services = new ServiceCollection()
            .AddAccessTokenProvider("first", _ => first)
            .AddAccessTokenProvider("second", _ => second);

        using var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IAccessTokenProviderFactory>();

        Assert.Same(first, factory.GetRequired("first"));
        Assert.Same(second, factory.GetRequired("second"));
        Assert.NotSame(factory.GetRequired("first"), factory.GetRequired("second"));
    }

    [Fact]
    public void AddAccessTokenProvider_WhenNameIsRegisteredAgain_UsesLastRegistration()
    {
        var first = new TestAccessTokenProvider("first");
        var second = new TestAccessTokenProvider("second");
        var firstFactoryCallCount = 0;
        var secondFactoryCallCount = 0;
        var services = new ServiceCollection()
            .AddAccessTokenProvider("shared", _ =>
            {
                firstFactoryCallCount++;
                return first;
            })
            .AddAccessTokenProvider("shared", _ =>
            {
                secondFactoryCallCount++;
                return second;
            });

        using var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IAccessTokenProviderFactory>();

        Assert.Same(second, factory.GetRequired("shared"));
        Assert.Same(second, factory.GetRequired("shared"));
        Assert.Equal(0, firstFactoryCallCount);
        Assert.Equal(1, secondFactoryCallCount);
    }

    [Fact]
    public void AddClientCredentialsTokenProvider_WithDifferentNames_CachesOneProviderPerName()
    {
        var services = new ServiceCollection()
            .AddHttpClient(nameof(ClientCredentialsTokenProvider))
            .Services
            .AddClientCredentialsTokenProvider("first", options => options.ClientId = "first-client")
            .AddClientCredentialsTokenProvider("second", options => options.ClientId = "second-client");

        using var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IAccessTokenProviderFactory>();

        var first = factory.GetRequired("first");
        var second = factory.GetRequired("second");

        Assert.IsType<ClientCredentialsTokenProvider>(first);
        Assert.IsType<ClientCredentialsTokenProvider>(second);
        Assert.NotSame(first, second);
        Assert.Same(first, factory.GetRequired("first"));
        Assert.Same(second, factory.GetRequired("second"));
    }

    [Fact]
    public async Task AddAuthenticationHandler_UsesProviderRegisteredWithName()
    {
        var tokenProvider = new TestAccessTokenProvider("named-token");
        var captureHandler = new CaptureAuthorizationHandler();
        var services = new ServiceCollection()
            .AddAccessTokenProvider("named", _ => tokenProvider)
            .AddHttpClient("api")
            .ConfigurePrimaryHttpMessageHandler(() => captureHandler)
            .AddAuthenticationHandler("named", ["api.read"])
            .Services;

        using var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("api");

        using var response = await client.GetAsync("https://example.com/resource");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Bearer", captureHandler.AuthorizationHeader?.Scheme);
        Assert.Equal("named-token", captureHandler.AuthorizationHeader?.Parameter);
        Assert.Equal(new[] { "api.read" }, Assert.Single(tokenProvider.RequestedScopes));
    }

    [Fact]
    public void GetRequired_WhenNameIsNotRegistered_Throws()
    {
        using var serviceProvider = new ServiceCollection()
            .AddAccessTokenProvider(_ => new TestAccessTokenProvider("default"))
            .BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IAccessTokenProviderFactory>();

        var exception = Assert.Throws<InvalidOperationException>(() => factory.GetRequired("missing"));

        Assert.Contains("missing", exception.Message);
    }

    [Fact]
    public void AccessTokenProviderFactory_DisposesCreatedProvidersOnce()
    {
        var disposeCount = 0;
        var provider = new DisposableAccessTokenProvider(() => disposeCount++);
        var serviceProvider = new ServiceCollection()
            .AddAccessTokenProvider("first", _ => provider)
            .AddAccessTokenProvider("second", _ => provider)
            .BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IAccessTokenProviderFactory>();
        factory.GetRequired("first");
        factory.GetRequired("second");

        serviceProvider.Dispose();

        Assert.Equal(1, disposeCount);
    }

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

    private sealed class DisposableAccessTokenProvider(Action onDispose) : IAccessTokenProvider, IDisposable
    {
        public Task<string> GetTokenAsync(string[] scopes, bool forceRefresh = false, CancellationToken cancellationToken = default)
        {
            return Task.FromResult("token");
        }

        public void Dispose() => onDispose();
    }
}
