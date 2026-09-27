namespace FclEx.Http;


/// <summary>
/// Registers named access token providers in a dependency injection container.
/// </summary>
public static partial class ServiceCollectionExtensions
{
    internal static IServiceCollection TrySetAccessTokenProvider<TProvider>(
        this IServiceCollection services,
        string name,
        Func<IServiceProvider, TProvider> factory,
        bool overwrite)
        where TProvider : class, IAccessTokenProvider
    {
        Check.NotNull(services);
        Check.NotNull(name);
        Check.NotNull(factory);

        services.AddOptions<AccessTokenProviderFactoryOptions>()
            .Configure(options => options.Factories.TrySet(name, factory, overwrite));
        services.TryAddSingleton<IAccessTokenProviderFactory, AccessTokenProviderFactory>();
        return services;
    }

    /// <summary>
    /// Registers the default access token provider.
    /// </summary>
    /// <remarks>
    /// The provider is created the first time it is requested and then cached for the lifetime of the service provider.
    /// Re-registering the empty name replaces the previous factory. Its factory receives the root service provider, so the
    /// provider should not capture scoped services. The factory owns the returned provider; do not return a disposable
    /// instance that is also managed by dependency injection. If the provider implements <see cref="IDisposable"/>, it is
    /// disposed when the service provider is disposed.
    /// </remarks>
    /// <typeparam name="TProvider">The provider implementation type.</typeparam>
    /// <param name="services">The service collection to add the provider registration to.</param>
    /// <param name="factory">Creates the provider when the default name is first requested.</param>
    /// <returns>The same service collection, for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="factory"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddAccessTokenProvider<TProvider>(
        this IServiceCollection services,
        Func<IServiceProvider, TProvider> factory)
        where TProvider : class, IAccessTokenProvider
    {
        return services.AddAccessTokenProvider(string.Empty, factory);
    }

    /// <summary>
    /// Registers a named access token provider.
    /// </summary>
    /// <remarks>
    /// The provider is created the first time it is requested and then cached for the lifetime of the service provider.
    /// Registering the same name more than once replaces its previous factory; registrations are not merged. The factory
    /// receives the root service provider, so the provider should not capture scoped services. The factory owns the returned
    /// provider; do not return a disposable instance that is also managed by dependency injection. If the provider implements
    /// <see cref="IDisposable"/>, it is disposed when the service provider is disposed.
    /// </remarks>
    /// <typeparam name="TProvider">The provider implementation type.</typeparam>
    /// <param name="services">The service collection to add the provider registration to.</param>
    /// <param name="name">The provider name. Names are compared using ordinal, case-sensitive comparison.</param>
    /// <param name="factory">Creates the provider when this name is first requested.</param>
    /// <returns>The same service collection, for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/>, <paramref name="name"/>, or <paramref name="factory"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddAccessTokenProvider<TProvider>(
        this IServiceCollection services,
        string name,
        Func<IServiceProvider, TProvider> factory)
        where TProvider : class, IAccessTokenProvider
    {
        return services.TrySetAccessTokenProvider(name, factory, true);
    }

    public static IServiceCollection TryAddAccessTokenProvider<TProvider>(
        this IServiceCollection services,
        string name,
        Func<IServiceProvider, TProvider> factory)
        where TProvider : class, IAccessTokenProvider
    {
        return services.TrySetAccessTokenProvider(name, factory, false);
    }

    /// <summary>
    /// Registers the default provider for OAuth/OIDC client-credentials token acquisition.
    /// </summary>
    /// <remarks>
    /// The options are applied when the provider is first requested. The provider is then cached for the lifetime of the
    /// service provider, retaining its discovery-document and access-token caches. Re-registering the default name replaces
    /// its previous configuration. This registration requires <see cref="IHttpClientFactory"/> to be registered, for example
    /// by calling <c>AddHttpClient</c>.
    /// </remarks>
    /// <param name="services">The service collection to add the provider registration to.</param>
    /// <param name="configureOptions">Configures the authority, client credentials, and discovery policy.</param>
    /// <returns>The same service collection, for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="configureOptions"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddClientCredentialsTokenProvider(
        this IServiceCollection services,
        Action<ClientCredentialsTokenProviderOptions> configureOptions)
    {
        return services.AddClientCredentialsTokenProvider(string.Empty, configureOptions);
    }

    /// <summary>
    /// Registers a named provider for OAuth/OIDC client-credentials token acquisition.
    /// </summary>
    /// <remarks>
    /// The options are applied when the provider is first requested. The provider is then cached for the lifetime of the
    /// service provider, retaining its discovery-document and access-token caches. Registering the same name more than once
    /// replaces its previous configuration; the configurations are not merged. Provider names are ordinal and case-sensitive.
    /// This registration requires <see cref="IHttpClientFactory"/> to be registered, for example by calling
    /// <c>AddHttpClient</c>. The provider uses the HTTP client named after <see cref="ClientCredentialsTokenProvider"/> for
    /// discovery and token requests; register that named client to customize its transport settings.
    /// </remarks>
    /// <param name="services">The service collection to add the provider registration to.</param>
    /// <param name="name">The provider name. Use an empty string for the default provider.</param>
    /// <param name="configureOptions">Configures the authority, client credentials, and discovery policy.</param>
    /// <returns>The same service collection, for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/>, <paramref name="name"/>, or <paramref name="configureOptions"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddClientCredentialsTokenProvider(
        this IServiceCollection services,
        string name,
        Action<ClientCredentialsTokenProviderOptions> configureOptions)
    {
        Check.NotNull(configureOptions);

        return services.AddAccessTokenProvider(name, serviceProvider =>
        {
            var options = new ClientCredentialsTokenProviderOptions();
            configureOptions(options);
            return new ClientCredentialsTokenProvider(options, serviceProvider.GetRequiredService<IHttpClientFactory>());
        });
    }

    public static IServiceCollection AddClientCredentialsTokenProvider(
        this IServiceCollection services,
        string name,
      Action<ClientCredentialsTokenProviderOptions, IServiceProvider> configureOptions)
    {
        Check.NotNull(configureOptions);
        return services.AddAccessTokenProvider(name, provider =>
        {
            var options = new ClientCredentialsTokenProviderOptions();
            configureOptions(options, provider);
            return new ClientCredentialsTokenProvider(options, provider.GetRequiredService<IHttpClientFactory>());
        });
    }

    public static IServiceCollection AddClientCredentialsTokenProvider(
        this IServiceCollection services,
        string name,
        Func<IServiceProvider, ClientCredentialsTokenProviderOptions> optionsFactory)
    {
        Check.NotNull(optionsFactory);
        return services.AddAccessTokenProvider(name, provider =>
        {
            var options = optionsFactory(provider);
            return new ClientCredentialsTokenProvider(options, provider.GetRequiredService<IHttpClientFactory>());
        });
    }

    public static IServiceCollection AddClientCredentialsTokenProviderBy<TDependency>(
        this IServiceCollection services,
        string name,
        Action<ClientCredentialsTokenProviderOptions, TDependency> configureOptions)
        where TDependency : class
    {
        Check.NotNull(configureOptions);
        return services.AddAccessTokenProvider(name, provider =>
        {
            var options = new ClientCredentialsTokenProviderOptions();
            var dependency = provider.GetRequiredService<TDependency>();
            configureOptions(options, dependency);
            return new ClientCredentialsTokenProvider(options, provider.GetRequiredService<IHttpClientFactory>());
        });
    }

    public static IServiceCollection AddClientCredentialsTokenProviderBy<TDependency>(
        this IServiceCollection services,
        string name,
        Func<TDependency, ClientCredentialsTokenProviderOptions> optionsFactory)
        where TDependency : class
    {
        Check.NotNull(optionsFactory);
        return services.AddAccessTokenProvider(name, provider =>
        {
            var dependency = provider.GetRequiredService<TDependency>();
            var options = optionsFactory(dependency);
            return new ClientCredentialsTokenProvider(options, provider.GetRequiredService<IHttpClientFactory>());
        });
    }

    public static IHttpClientBuilder AddAuthenticationHandler(
        this IHttpClientBuilder builder,
        Action<AuthenticationHandlerOptions> configureOptions)
    {
        Check.NotNull(builder);
        Check.NotNull(configureOptions);

        return builder.AddAuthenticationHandler(provider =>
        {
            var options = new AuthenticationHandlerOptions();
            configureOptions(options);
            return options;
        });
    }

    public static IHttpClientBuilder AddAuthenticationHandler(
        this IHttpClientBuilder builder,
        Action<AuthenticationHandlerOptions, IServiceProvider> configureOptions)
    {
        Check.NotNull(builder);
        Check.NotNull(configureOptions);

        return builder.AddAuthenticationHandler(provider =>
        {
            var options = new AuthenticationHandlerOptions();
            configureOptions(options, provider);
            return options;
        });
    }

    public static IHttpClientBuilder AddAuthenticationHandlerBy<TDependency>(
        this IHttpClientBuilder builder,
        Func<TDependency, AuthenticationHandlerOptions> optionsFactory)
        where TDependency : class
    {
        Check.NotNull(builder);
        Check.NotNull(optionsFactory);

        return builder.AddAuthenticationHandler(provider =>
        {
            var dependency = provider.GetRequiredService<TDependency>();
            var options = optionsFactory(dependency);
            return options;
        });
    }

    public static IHttpClientBuilder AddAuthenticationHandlerBy<TDependency>(
        this IHttpClientBuilder builder,
        Action<AuthenticationHandlerOptions, TDependency> optionsFactory)
        where TDependency : class
    {
        Check.NotNull(builder);
        Check.NotNull(optionsFactory);

        return builder.AddAuthenticationHandlerBy<TDependency>(dependency =>
        {
            var options = new AuthenticationHandlerOptions();
            optionsFactory(options, dependency);
            return options;
        });
    }
}