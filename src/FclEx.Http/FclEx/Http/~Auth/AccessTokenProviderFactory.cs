namespace FclEx.Http;

/// <summary>
/// Resolves the long-lived access token provider registered for a name.
/// </summary>
public interface IAccessTokenProviderFactory
{
    /// <summary>
    /// Gets the provider registered under <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The provider name. Use an empty string for the default provider.</param>
    /// <returns>The cached provider instance for that name.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">No provider is registered with that name.</exception>
    /// <exception cref="ObjectDisposedException">The service provider has been disposed.</exception>
    IAccessTokenProvider GetRequired(string name);
}

internal sealed class AccessTokenProviderFactoryOptions
{
    public Dictionary<string, Func<IServiceProvider, IAccessTokenProvider>> Factories { get; } = new(StringComparer.Ordinal);
}

internal sealed class AccessTokenProviderFactory(
    IServiceProvider serviceProvider,
    IOptions<AccessTokenProviderFactoryOptions> options) : IAccessTokenProviderFactory, IDisposable
{
    private static readonly
#if NET9_0_OR_GREATER
        Lock
#else
        object
#endif
    _lock = new();
    private readonly Dictionary<string, IAccessTokenProvider> _providers = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Func<IServiceProvider, IAccessTokenProvider>> _factories = options.Value.Factories;
    private bool _disposed;

    public IAccessTokenProvider GetRequired(string name)
    {
        Check.NotNull(name);

        if (_factories.TryGetValue(name, out var factory) == false)
            throw new InvalidOperationException($"No access token provider is registered with the name '{name}'.");

        lock (_lock)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AccessTokenProviderFactory));

            if (_providers.TryGetValue(name, out var provider))
                return provider;

            provider = Check.NotNull(factory(serviceProvider));
            _providers.Add(name, provider);
            return provider;
        }
    }

    public void Dispose()
    {
        IDisposable[] disposables;
        lock (_lock)
        {
            if (_disposed)
                return;

            _disposed = true;
            var uniqueDisposables = new HashSet<IDisposable>(ReferenceEqualityComparer.Instance);
            foreach (var provider in _providers.Values)
            {
                // ReSharper disable once SuspiciousTypeConversion.Global
                if (provider is IDisposable disposable)
                    uniqueDisposables.Add(disposable);
            }
            disposables = uniqueDisposables.ToArray();
            _providers.Clear();
        }

        foreach (var disposable in disposables)
            disposable.Dispose();
    }

}

/// <summary>
/// Registers named access token providers in a dependency injection container.
/// </summary>
public static class AccessTokenProviderServiceCollectionExtensions
{
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
        Check.NotNull(services);
        Check.NotNull(name);
        Check.NotNull(factory);

        services.AddOptions<AccessTokenProviderFactoryOptions>()
            .Configure(options => options.Factories[name] = serviceProvider => factory(serviceProvider));
        services.TryAddSingleton<IAccessTokenProviderFactory, AccessTokenProviderFactory>();
        return services;
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
}
