namespace FclEx.Http.Extensions;

public class HttpClientBuilderExtensionsTests
{
    [Fact]
    public async Task AddHttpMessageHandlerBy_ResolvesDependencyWhenBuildingHandler()
    {
        var dependency = new HandlerDependency("from-dependency");
        var primary = new CaptureRequestHandler();
        var services = new ServiceCollection();
        services.AddSingleton(dependency);
        services.AddHttpClient("handler-by")
            .ConfigurePrimaryHttpMessageHandler(() => primary)
            .AddHttpMessageHandlerBy<HandlerDependency>(m => new DependencyHeaderHandler(m));
        using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient("handler-by");

        using var response = await client.GetAsync("https://example.test/path");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(dependency.HandlerWasCalled);
        Assert.Equal("from-dependency", primary.Request?.Headers.GetValues("X-Dependency").Single());
    }

    [Fact]
    public async Task AddPolicyHandlerBy_WhenDependencyFactoryIsProvided_UsesFactoryResultAndRequest()
    {
        var dependency = new PolicyDependency();
        var services = new ServiceCollection();
        services.AddHttpClient("policy-by-factory")
            .ConfigurePrimaryHttpMessageHandler(() => new CaptureRequestHandler())
            .AddPolicyHandlerBy(_ => dependency, (m, request) =>
            {
                m.FactoryCallCount++;
                m.RequestUri = request.RequestUri;
                return Polly.Policy.NoOpAsync<HttpResponseMessage>();
            });
        using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient("policy-by-factory");

        using var response = await client.GetAsync("https://example.test/policy");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, dependency.FactoryCallCount);
        Assert.Equal(new Uri("https://example.test/policy"), dependency.RequestUri);
    }

    [Fact]
    public async Task AddPolicyHandlerBy_WhenDependencyIsRegistered_ResolvesDependencyFromServices()
    {
        var dependency = new PolicyDependency();
        var services = new ServiceCollection();
        services.AddSingleton(dependency);
        services.AddHttpClient("policy-by-registered-dependency")
            .ConfigurePrimaryHttpMessageHandler(() => new CaptureRequestHandler())
            .AddPolicyHandlerBy<PolicyDependency>((m, request) =>
            {
                m.FactoryCallCount++;
                m.RequestUri = request.RequestUri;
                return Polly.Policy.NoOpAsync<HttpResponseMessage>();
            });
        using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient("policy-by-registered-dependency");

        using var response = await client.GetAsync("https://example.test/registered");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, dependency.FactoryCallCount);
        Assert.Equal(new Uri("https://example.test/registered"), dependency.RequestUri);
    }

    [Fact]
    public void AddRetryPolicy_WhenAutoUpdateTotalTimeoutIsTrue_IncreasesShorterHttpClientTimeout()
    {
        var services = new ServiceCollection();
        services.AddHttpClient("retry-auto-timeout", m => m.Timeout = TimeSpan.FromMilliseconds(1))
            .ConfigurePrimaryHttpMessageHandler(() => new CaptureRequestHandler())
            .AddRetryPolicy(new HttpClientRetryPolicyOptions
            {
                ExecutionTimeout = TimeSpan.FromSeconds(2),
                RetryCount = 1,
                AutoUpdateTotalTimeout = true,
                SleepDurationProvider = _ => TimeSpan.FromSeconds(3),
            });
        using var provider = services.BuildServiceProvider();

        var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient("retry-auto-timeout");

        Assert.Equal(TimeSpan.FromSeconds(8), client.Timeout);
    }

    [Fact]
    public void AddRetryPolicy_WhenAutoUpdateTotalTimeoutIsFalse_KeepsConfiguredHttpClientTimeout()
    {
        var expectedTimeout = TimeSpan.FromSeconds(7);
        var services = new ServiceCollection();
        services.AddHttpClient("retry-keep-timeout", m => m.Timeout = expectedTimeout)
            .ConfigurePrimaryHttpMessageHandler(() => new CaptureRequestHandler())
            .AddRetryPolicy(new HttpClientRetryPolicyOptions
            {
                ExecutionTimeout = TimeSpan.FromSeconds(2),
                RetryCount = 1,
                AutoUpdateTotalTimeout = false,
                SleepDurationProvider = _ => TimeSpan.FromSeconds(3),
            });
        using var provider = services.BuildServiceProvider();

        var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient("retry-keep-timeout");

        Assert.Equal(expectedTimeout, client.Timeout);
    }

    [Fact]
    public void AddRetryPolicy_WhenOptionsFactoryIsUsed_UsesResolvedOptionsToUpdateTimeout()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new RetryOptionsProvider
        {
            Options = new()
            {
                ExecutionTimeout = TimeSpan.FromSeconds(1),
                RetryCount = 2,
                AutoUpdateTotalTimeout = true,
                SleepDurationProvider = _ => TimeSpan.FromSeconds(2),
            },
        });
        services.AddHttpClient("retry-options-factory", m => m.Timeout = TimeSpan.FromMilliseconds(1))
            .ConfigurePrimaryHttpMessageHandler(() => new CaptureRequestHandler())
            .AddRetryPolicy(m => m.GetRequiredService<RetryOptionsProvider>().Options);
        using var provider = services.BuildServiceProvider();

        var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient("retry-options-factory");

        Assert.Equal(TimeSpan.FromSeconds(8), client.Timeout);
    }

    private sealed record HandlerDependency(string HeaderValue)
    {
        public bool HandlerWasCalled { get; set; }
    }

    private sealed class DependencyHeaderHandler(HandlerDependency dependency) : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            dependency.HandlerWasCalled = true;
            request.Headers.Add("X-Dependency", dependency.HeaderValue);
            return base.SendAsync(request, cancellationToken);
        }
    }

    private sealed class PolicyDependency
    {
        public int FactoryCallCount { get; set; }
        public Uri? RequestUri { get; set; }
    }

    private sealed class RetryOptionsProvider
    {
        public HttpClientRetryPolicyOptions Options { get; init; } = new();
    }

    private sealed class CaptureRequestHandler : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                RequestMessage = request,
            });
        }
    }
}
