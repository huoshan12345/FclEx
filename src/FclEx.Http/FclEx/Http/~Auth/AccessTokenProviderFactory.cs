namespace FclEx.Http;

public interface IAccessTokenProviderFactory
{
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

public static class AccessTokenProviderServiceCollectionExtensions
{
    public static IServiceCollection AddAccessTokenProvider<TProvider>(
        this IServiceCollection services,
        Func<IServiceProvider, TProvider> factory)
        where TProvider : class, IAccessTokenProvider
    {
        return services.AddAccessTokenProvider(string.Empty, factory);
    }

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

    public static IServiceCollection AddClientCredentialsTokenProvider(
        this IServiceCollection services,
        Action<ClientCredentialsTokenProviderOptions> configureOptions)
    {
        return services.AddClientCredentialsTokenProvider(string.Empty, configureOptions);
    }

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
