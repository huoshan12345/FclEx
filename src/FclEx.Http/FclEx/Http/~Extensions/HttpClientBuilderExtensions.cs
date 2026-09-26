namespace FclEx.Http;

/// <summary>
/// Extensions for registering HTTP clients with FclEx retry policies and dependency-aware handlers.
/// </summary>
public static partial class HttpClientBuilderExtensions
{
    /// <summary>
    /// Adds the package retry, timeout, connect-timeout, and IO retry policies to an HTTP client builder.
    /// When enabled by options, the registered <see cref="HttpClient.Timeout"/> is raised to cover the maximum retry window.
    /// </summary>
    public static IHttpClientBuilder AddRetryPolicy(this IHttpClientBuilder builder, HttpClientRetryPolicyOptions options)
    {
        if (options.AutoUpdateTotalTimeout)
        {
            builder.ConfigureHttpClient(httpClient =>
            {
                var totalTimeout = ComputeMaxTimeout(options);
                if (httpClient.Timeout < totalTimeout)
                    httpClient.Timeout = totalTimeout;
            });
        }

        builder.AddPolicyHandler(PollyHelper.GetHttpRetryPolicy(options.RetryCount, options.SleepDurationProvider));
        builder.AddPolicyHandler(PollyHelper.GetTimeoutPolicy(options.ExecutionTimeout));
        builder.AddPolicyHandler(PollyHelper.GetConnectTimeoutPolicy(options.RetryCount));
        builder.AddPolicyHandler(PollyHelper.GetIORetryPolicy(options.RetryCount));
        return builder;
    }

    /// <summary>
    /// Adds a delegating handler built from a dependency resolved from the request service provider.
    /// </summary>
    public static IHttpClientBuilder AddHttpMessageHandlerBy<TDependency>(this IHttpClientBuilder builder, Func<TDependency, DelegatingHandler> configureHandler)
        where TDependency : class
    {
        return builder.AddHttpMessageHandler(m => configureHandler(m.GetRequiredService<TDependency>()));
    }

    /// <summary>
    /// Adds an <see cref="AuthenticationHandler"/> that uses the default access token provider.
    /// </summary>
    /// <remarks>
    /// The default provider is the provider registered with an empty name. Register it with
    /// <see cref="AccessTokenProviderServiceCollectionExtensions.AddClientCredentialsTokenProvider(IServiceCollection, Action{ClientCredentialsTokenProviderOptions})"/>
    /// or <see cref="AccessTokenProviderServiceCollectionExtensions.AddAccessTokenProvider{TProvider}(IServiceCollection, Func{IServiceProvider, TProvider})"/>.
    /// The handler is created when <see cref="IHttpClientFactory"/> builds the client's handler chain.
    /// </remarks>
    /// <param name="builder">The HTTP client builder to add the authentication handler to.</param>
    /// <param name="scopes">The scopes requested from the provider. <see langword="null"/> is treated as an empty scope list.</param>
    /// <param name="requireToken">
    /// Whether the handler should request and attach a bearer token. When <see langword="false"/>, requests are forwarded
    /// without token acquisition, but the default provider registration is still resolved when the handler chain is built.
    /// </param>
    /// <returns>The same builder, for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">No default access token provider is registered when the handler chain is built.</exception>
    public static IHttpClientBuilder AddAuthenticationHandler(
        this IHttpClientBuilder builder,
        string[]? scopes = null,
        bool requireToken = true)
    {
        return builder.AddAuthenticationHandler(string.Empty, scopes, requireToken);
    }

    /// <summary>
    /// Adds an <see cref="AuthenticationHandler"/> that uses the named access token provider.
    /// </summary>
    /// <remarks>
    /// Register a client-credentials provider with the same name using
    /// <see cref="AccessTokenProviderServiceCollectionExtensions.AddClientCredentialsTokenProvider(IServiceCollection, string, Action{ClientCredentialsTokenProviderOptions})"/>,
    /// or register a custom implementation using
    /// <see cref="AccessTokenProviderServiceCollectionExtensions.AddAccessTokenProvider{TProvider}(IServiceCollection, string, Func{IServiceProvider, TProvider})"/>.
    /// The provider name is independent of the HTTP client's name, so multiple clients can share one provider. The handler
    /// is created when <see cref="IHttpClientFactory"/> builds the client's handler chain.
    /// </remarks>
    /// <param name="builder">The HTTP client builder to add the authentication handler to.</param>
    /// <param name="tokenProviderName">The name used to register the provider. Use an empty string for the default provider.</param>
    /// <param name="scopes">The scopes requested from the provider. <see langword="null"/> is treated as an empty scope list.</param>
    /// <param name="requireToken">
    /// Whether the handler should request and attach a bearer token. When <see langword="false"/>, requests are forwarded
    /// without token acquisition, but the named provider registration is still resolved when the handler chain is built.
    /// </param>
    /// <returns>The same builder, for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> or <paramref name="tokenProviderName"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">No provider is registered with <paramref name="tokenProviderName"/> when the handler chain is built.</exception>
    public static IHttpClientBuilder AddAuthenticationHandler(
        this IHttpClientBuilder builder,
        string tokenProviderName,
        string[]? scopes = null,
        bool requireToken = true)
    {
        Check.NotNull(builder);
        Check.NotNull(tokenProviderName);

        return builder.AddHttpMessageHandler(serviceProvider => new AuthenticationHandler(
            serviceProvider.GetRequiredService<IAccessTokenProviderFactory>().GetRequired(tokenProviderName),
            scopes,
            requireToken));
    }

    /// <summary>
    /// Adds a policy handler built from a dependency supplied by a factory.
    /// The dependency is resolved each time the HTTP client factory asks for a policy for a request.
    /// </summary>
    public static IHttpClientBuilder AddPolicyHandlerBy<TDependency>(
        this IHttpClientBuilder builder,
        Func<IServiceProvider, TDependency> dependencyFactory,
        Func<TDependency, HttpRequestMessage, IAsyncPolicy<HttpResponseMessage>> policyFactory)
        where TDependency : class
    {
        return builder.AddPolicyHandler((serviceProvider, request) =>
        {
            var dependency = dependencyFactory(serviceProvider);
            return policyFactory(dependency, request);
        });
    }

    /// <summary>
    /// Adds a policy handler built from a dependency resolved from the request service provider.
    /// </summary>
    public static IHttpClientBuilder AddPolicyHandlerBy<TDependency>(
        this IHttpClientBuilder builder,
        Func<TDependency, HttpRequestMessage, IAsyncPolicy<HttpResponseMessage>> policyFactory)
        where TDependency : class
    {
        return builder.AddPolicyHandlerBy(s => s.GetRequiredService<TDependency>(), policyFactory);
    }

    /// <summary>
    /// Adds retry policies whose options are resolved from the service provider during client configuration and policy creation.
    /// </summary>
    public static IHttpClientBuilder AddRetryPolicy(this IHttpClientBuilder builder, Func<IServiceProvider, HttpClientRetryPolicyOptions> optionsFactory)
    {
        builder.ConfigureHttpClient((serviceProvider, httpClient) =>
        {
            var options = optionsFactory(serviceProvider);
            if (options.AutoUpdateTotalTimeout == false)
                return;

            var totalTimeout = ComputeMaxTimeout(options);
            if (httpClient.Timeout < totalTimeout)
                httpClient.Timeout = totalTimeout;
        });

        builder.AddPolicyHandlerBy(optionsFactory, (options, _) => PollyHelper.GetHttpRetryPolicy(options.RetryCount, options.SleepDurationProvider));
        builder.AddPolicyHandlerBy(optionsFactory, (options, _) => PollyHelper.GetTimeoutPolicy(options.ExecutionTimeout));
        builder.AddPolicyHandlerBy(optionsFactory, (options, _) => PollyHelper.GetConnectTimeoutPolicy(options.RetryCount));
        builder.AddPolicyHandlerBy(optionsFactory, (options, _) => PollyHelper.GetIORetryPolicy(options.RetryCount));
        return builder;
    }

    private static TimeSpan ComputeMaxTimeout(HttpClientRetryPolicyOptions options)
    {
        var timeout = options.ExecutionTimeout;

        var totalTimeout = timeout + TimeSpan.FromSeconds(1);
        for (var i = 0; i < options.RetryCount; i++)
        {
            totalTimeout += timeout;
            totalTimeout += options.SleepDurationProvider(i + 1);
        }

        return totalTimeout;
    }
}
