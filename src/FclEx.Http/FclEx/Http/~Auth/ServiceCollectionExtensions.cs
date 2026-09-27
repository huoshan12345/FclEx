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
    /// Re-registering the empty name replaces the previous factory. The callback runs lazily on first resolution and
    /// receives the root service provider, so it must not resolve scoped services. The factory owns the returned provider;
    /// do not return a disposable instance that is also managed by dependency injection. If the provider implements
    /// <see cref="IDisposable"/>, it is disposed when the service provider is disposed.
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
    /// runs lazily on first resolution and receives the root service provider, so it must not resolve scoped services. The
    /// factory owns the returned provider; do not return a disposable instance that is also managed by dependency injection.
    /// If the provider implements <see cref="IDisposable"/>, it is disposed when the service provider is disposed.
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

    /// <summary>
    /// Registers a named access token provider only when no provider factory has already been registered under that name.
    /// </summary>
    /// <remarks>
    /// The provider is created lazily on first resolution and cached for the lifetime of the service provider. Names are
    /// compared using ordinal, case-sensitive comparison. The factory receives the root service provider and must not
    /// resolve scoped services. The factory owns the returned provider and disposes it with the service provider if it
    /// implements <see cref="IDisposable"/>.
    /// </remarks>
    /// <typeparam name="TProvider">The provider implementation type.</typeparam>
    /// <param name="services">The service collection to add the provider registration to.</param>
    /// <param name="name">The provider name. Use an empty string for the default provider.</param>
    /// <param name="factory">Creates the provider when this name is first requested.</param>
    /// <returns>The same service collection, for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/>, <paramref name="name"/>, or <paramref name="factory"/> is <see langword="null"/>.</exception>
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
    /// The options callback runs lazily the first time the default provider is requested. The provider is then cached for
    /// the lifetime of the service provider, retaining its discovery-document and access-token caches. Re-registering the
    /// default name replaces its previous configuration. This registration requires <see cref="IHttpClientFactory"/> to be
    /// registered, for example by calling <c>AddHttpClient</c>.
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
    /// The options callback runs lazily the first time the named provider is requested. The provider is then cached for the
    /// lifetime of the service provider, retaining its discovery-document and access-token caches. Registering the same name
    /// more than once replaces its previous configuration; the configurations are not merged. Provider names are ordinal
    /// and case-sensitive.
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

    /// <summary>
    /// Registers a named client-credentials provider configured with services from the root service provider.
    /// </summary>
    /// <remarks>
    /// The callback runs lazily the first time the provider is requested, receives the root service provider, and runs once
    /// for that provider. Do not resolve scoped services from it. The resulting provider is cached for the service provider
    /// lifetime. If both this overload and the options-factory overload could match a lambda, use an explicitly typed
    /// delegate to select the intended overload. Registering the same name again replaces the previous registration.
    /// </remarks>
    /// <param name="services">The service collection to add the provider registration to.</param>
    /// <param name="name">The provider name. Use an empty string for the default provider.</param>
    /// <param name="configureOptions">Configures the authority, client credentials, and discovery policy using root services.</param>
    /// <returns>The same service collection, for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/>, <paramref name="name"/>, or <paramref name="configureOptions"/> is <see langword="null"/>.</exception>
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

    /// <summary>
    /// Registers a named client-credentials provider whose options are created from the root service provider.
    /// </summary>
    /// <remarks>
    /// The options factory runs lazily the first time the provider is requested, receives the root service provider, and runs
    /// once for that provider. Do not resolve scoped services from it. The resulting provider is cached for the service
    /// provider lifetime. Registering the same name again replaces the previous registration.
    /// </remarks>
    /// <param name="services">The service collection to add the provider registration to.</param>
    /// <param name="name">The provider name. Use an empty string for the default provider.</param>
    /// <param name="optionsFactory">Creates options containing the authority, client credentials, and discovery policy.</param>
    /// <returns>The same service collection, for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/>, <paramref name="name"/>, or <paramref name="optionsFactory"/> is <see langword="null"/>.</exception>
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

    /// <summary>
    /// Registers a named client-credentials provider configured using a registered dependency.
    /// </summary>
    /// <remarks>
    /// The dependency and callback are resolved or invoked lazily once when the provider is first requested. The dependency
    /// is resolved from the root service provider, so it must not be scoped. The provider is cached for the service provider
    /// lifetime. Registering the same name again replaces the previous registration.
    /// </remarks>
    /// <typeparam name="TDependency">The registered dependency used to configure the provider.</typeparam>
    /// <param name="services">The service collection to add the provider registration to.</param>
    /// <param name="name">The provider name. Use an empty string for the default provider.</param>
    /// <param name="configureOptions">Configures the authority, client credentials, and discovery policy.</param>
    /// <returns>The same service collection, for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/>, <paramref name="name"/>, or <paramref name="configureOptions"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException"><typeparamref name="TDependency"/> is not registered.</exception>
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

    /// <summary>
    /// Registers a named client-credentials provider whose options are created from a registered dependency.
    /// </summary>
    /// <remarks>
    /// The dependency and options factory are resolved or invoked lazily once when the provider is first requested. The
    /// dependency is resolved from the root service provider, so it must not be scoped. The provider is cached for the service
    /// provider lifetime. Registering the same name again replaces the previous registration.
    /// </remarks>
    /// <typeparam name="TDependency">The registered dependency used to create the options.</typeparam>
    /// <param name="services">The service collection to add the provider registration to.</param>
    /// <param name="name">The provider name. Use an empty string for the default provider.</param>
    /// <param name="optionsFactory">Creates options containing the authority, client credentials, and discovery policy.</param>
    /// <returns>The same service collection, for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/>, <paramref name="name"/>, or <paramref name="optionsFactory"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException"><typeparamref name="TDependency"/> is not registered.</exception>
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

    /// <summary>
    /// Adds an authentication handler whose options are configured without resolving additional services.
    /// </summary>
    /// <remarks>The handler resolves the named token provider when the HTTP client's handler chain is built.</remarks>
    /// <param name="builder">The HTTP client builder to add the handler to.</param>
    /// <param name="configureOptions">Configures the provider name, scopes, and token requirement.</param>
    /// <returns>The same builder, for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> or <paramref name="configureOptions"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The configured access token provider is not registered.</exception>
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

    /// <summary>
    /// Adds an authentication handler whose options are configured with the handler service provider.
    /// </summary>
    /// <remarks>
    /// The options callback runs when the HTTP client's handler chain is built. Its service provider is the handler scope;
    /// the selected access token provider is still obtained from the registered provider factory.
    /// </remarks>
    /// <param name="builder">The HTTP client builder to add the handler to.</param>
    /// <param name="configureOptions">Configures the provider name, scopes, and token requirement using handler-scope services.</param>
    /// <returns>The same builder, for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> or <paramref name="configureOptions"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The configured access token provider is not registered.</exception>
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

    /// <summary>
    /// Adds an authentication handler whose options are created from a registered dependency.
    /// </summary>
    /// <remarks>The dependency is resolved from the handler scope when the HTTP client's handler chain is built.</remarks>
    /// <typeparam name="TDependency">The registered dependency used to create the options.</typeparam>
    /// <param name="builder">The HTTP client builder to add the handler to.</param>
    /// <param name="optionsFactory">Creates the handler options, including the access token provider name.</param>
    /// <returns>The same builder, for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> or <paramref name="optionsFactory"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException"><typeparamref name="TDependency"/> or the selected provider is not registered.</exception>
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

    /// <summary>
    /// Adds an authentication handler whose options are configured using a registered dependency.
    /// </summary>
    /// <remarks>The dependency is resolved from the handler scope when the HTTP client's handler chain is built.</remarks>
    /// <typeparam name="TDependency">The registered dependency used to configure the options.</typeparam>
    /// <param name="builder">The HTTP client builder to add the handler to.</param>
    /// <param name="optionsFactory">Configures the provider name, scopes, and token requirement.</param>
    /// <returns>The same builder, for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> or <paramref name="optionsFactory"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException"><typeparamref name="TDependency"/> or the selected provider is not registered.</exception>
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
