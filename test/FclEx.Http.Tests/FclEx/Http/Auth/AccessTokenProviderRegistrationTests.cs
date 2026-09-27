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
    public void TryAddAccessTokenProvider_WhenNameAlreadyExists_KeepsExistingProvider()
    {
        var first = new TestAccessTokenProvider("first");
        var second = new TestAccessTokenProvider("second");
        var services = new ServiceCollection()
            .AddAccessTokenProvider("shared", _ => first)
            .TryAddAccessTokenProvider("shared", _ => second);

        using var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IAccessTokenProviderFactory>();

        Assert.Same(first, factory.GetRequired("shared"));
    }

    [Fact]
    public void TryAddAccessTokenProvider_WhenNameIsNotRegistered_AddsProvider()
    {
        var expected = new TestAccessTokenProvider("new");
        var services = new ServiceCollection()
            .TryAddAccessTokenProvider("new", _ => expected);

        using var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IAccessTokenProviderFactory>();

        Assert.Same(expected, factory.GetRequired("new"));
    }

    [Fact]
    public void TryAddAccessTokenProvider_WithoutName_RegistersDefaultOnlyWhenMissing()
    {
        var expected = new TestAccessTokenProvider("default");
        var services = new ServiceCollection()
            .TryAddAccessTokenProvider(_ => expected)
            .TryAddAccessTokenProvider<TestAccessTokenProvider>(_ => throw new InvalidOperationException("The existing registration should be kept."));

        using var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IAccessTokenProviderFactory>();

        Assert.Same(expected, factory.GetRequired(string.Empty));
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
    public void AddClientCredentialsTokenProvider_WithServiceCallbacks_CreatesAndCachesConfiguredProviders()
    {
        var serviceOptions = new ClientCredentialsTokenProviderOptions { ClientId = "service-provider" };
        var callbacks = new Dictionary<string, int>();
        var services = new ServiceCollection()
            .AddSingleton(serviceOptions)
            .AddHttpClient(nameof(ClientCredentialsTokenProvider))
            .Services
            .AddClientCredentialsTokenProvider("service-callback", new Action<ClientCredentialsTokenProviderOptions, IServiceProvider>((options, provider) =>
            {
                Increment(callbacks, "service-callback");
                options.ClientId = provider.GetRequiredService<ClientCredentialsTokenProviderOptions>().ClientId;
            }))
            .AddClientCredentialsTokenProvider("service-factory", new Func<IServiceProvider, ClientCredentialsTokenProviderOptions>(provider =>
            {
                Increment(callbacks, "service-factory");
                return new() { ClientId = provider.GetRequiredService<ClientCredentialsTokenProviderOptions>().ClientId };
            }))
            .AddClientCredentialsTokenProviderBy<ClientCredentialsTokenProviderOptions>("dependency-callback", (options, dependency) =>
            {
                Increment(callbacks, "dependency-callback");
                options.ClientId = dependency.ClientId;
            })
            .AddClientCredentialsTokenProviderBy<ClientCredentialsTokenProviderOptions>("dependency-factory", dependency =>
            {
                Increment(callbacks, "dependency-factory");
                return new() { ClientId = dependency.ClientId };
            });

        using var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IAccessTokenProviderFactory>();

        var names = new[] { "service-callback", "service-factory", "dependency-callback", "dependency-factory" };
        foreach (var name in names)
        {
            var provider = factory.GetRequired(name);
            Assert.Equal("service-provider", GetOptions(provider).ClientId);
            Assert.Same(provider, factory.GetRequired(name));
            Assert.Equal(1, callbacks[name]);
        }
    }

    [Fact]
    public void TryAddClientCredentialsTokenProvider_WhenNamesAreMissing_RegistersAllOverloads()
    {
        var dependencyOptions = new ClientCredentialsTokenProviderOptions { ClientId = "dependency" };
        var services = new ServiceCollection()
            .AddSingleton(dependencyOptions)
            .AddHttpClient(nameof(ClientCredentialsTokenProvider))
            .Services
            .TryAddClientCredentialsTokenProvider(new Action<ClientCredentialsTokenProviderOptions>(options => options.ClientId = "default"))
            .TryAddClientCredentialsTokenProvider("named", new Action<ClientCredentialsTokenProviderOptions>(options => options.ClientId = "named"))
            .TryAddClientCredentialsTokenProvider("service-callback", new Action<ClientCredentialsTokenProviderOptions, IServiceProvider>((options, provider) =>
            {
                options.ClientId = provider.GetRequiredService<ClientCredentialsTokenProviderOptions>().ClientId;
            }))
            .TryAddClientCredentialsTokenProvider("service-factory", new Func<IServiceProvider, ClientCredentialsTokenProviderOptions>(provider =>
                new() { ClientId = provider.GetRequiredService<ClientCredentialsTokenProviderOptions>().ClientId }))
            .TryAddClientCredentialsTokenProviderBy<ClientCredentialsTokenProviderOptions>("dependency-callback", (options, dependency) =>
            {
                options.ClientId = dependency.ClientId;
            })
            .TryAddClientCredentialsTokenProviderBy<ClientCredentialsTokenProviderOptions>("dependency-factory", dependency =>
                new() { ClientId = dependency.ClientId });

        using var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IAccessTokenProviderFactory>();
        var expectedClients = new Dictionary<string, string>
        {
            [string.Empty] = "default",
            ["named"] = "named",
            ["service-callback"] = "dependency",
            ["service-factory"] = "dependency",
            ["dependency-callback"] = "dependency",
            ["dependency-factory"] = "dependency",
        };

        foreach (var (name, expectedClientId) in expectedClients)
        {
            var provider = factory.GetRequired(name);
            Assert.Equal(expectedClientId, GetOptions(provider).ClientId);
            Assert.Same(provider, factory.GetRequired(name));
        }
    }

    [Fact]
    public void TryAddClientCredentialsTokenProvider_WhenNamesAlreadyExist_KeepsExistingProvidersWithoutInvokingCallbacks()
    {
        var names = new[] { string.Empty, "named", "service-callback", "service-factory", "dependency-callback", "dependency-factory" };
        var existingProviders = names.ToDictionary(name => name, name => new TestAccessTokenProvider(name));
        var callbackCount = 0;
        var services = new ServiceCollection();
        foreach (var (name, provider) in existingProviders)
            services.AddAccessTokenProvider(name, _ => provider);

        services
            .TryAddClientCredentialsTokenProvider(new Action<ClientCredentialsTokenProviderOptions>(_ => callbackCount++))
            .TryAddClientCredentialsTokenProvider("named", new Action<ClientCredentialsTokenProviderOptions>(_ => callbackCount++))
            .TryAddClientCredentialsTokenProvider("service-callback", new Action<ClientCredentialsTokenProviderOptions, IServiceProvider>((_, _) => callbackCount++))
            .TryAddClientCredentialsTokenProvider("service-factory", new Func<IServiceProvider, ClientCredentialsTokenProviderOptions>(_ =>
            {
                callbackCount++;
                return new();
            }))
            .TryAddClientCredentialsTokenProviderBy<UnregisteredDependency>("dependency-callback", (_, _) => callbackCount++)
            .TryAddClientCredentialsTokenProviderBy<UnregisteredDependency>("dependency-factory", _ =>
            {
                callbackCount++;
                return new();
            });

        using var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IAccessTokenProviderFactory>();

        foreach (var (name, provider) in existingProviders)
            Assert.Same(provider, factory.GetRequired(name));
        Assert.Equal(0, callbackCount);
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

    private sealed class UnregisteredDependency
    {
    }

    private static ClientCredentialsTokenProviderOptions GetOptions(IAccessTokenProvider provider)
    {
        return typeof(ClientCredentialsTokenProvider)
            .GetRequiredField("_options")
            .GetRequiredValue<ClientCredentialsTokenProviderOptions>(provider);
    }

    private static void Increment(Dictionary<string, int> counts, string key)
    {
        counts[key] = counts.TryGetValue(key, out var count) ? count + 1 : 1;
    }
}
