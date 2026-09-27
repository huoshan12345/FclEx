namespace FclEx.Http;

/// <summary>
/// Holds the factories used to create named <see cref="IAccessTokenProvider"/> instances.
/// </summary>
/// <remarks>
/// Registrations are stored by ordinal, case-sensitive name. The service collection registration extensions populate this
/// type, and <see cref="AccessTokenProviderFactory"/> uses the factories to create and cache providers.
/// </remarks>
public sealed class AccessTokenProviderFactoryOptions
{
    /// <summary>
    /// Gets the provider factories keyed by provider name.
    /// </summary>
    /// <remarks>Each factory is invoked at most once per name by an <see cref="AccessTokenProviderFactory"/> instance.</remarks>
    public Dictionary<string, Func<IServiceProvider, IAccessTokenProvider>> Factories { get; } = new(StringComparer.Ordinal);
}

/// <summary>
/// Creates and caches access token providers registered under names.
/// </summary>
/// <remarks>
/// A provider is created on the first request for its name and reused for subsequent requests. This factory owns created
/// providers that implement <see cref="IDisposable"/> and disposes each distinct instance when the factory is disposed.
/// </remarks>
/// <param name="serviceProvider">The root service provider passed to provider registration callbacks.</param>
/// <param name="options">The registered provider factories.</param>
public sealed class AccessTokenProviderFactory(
    IServiceProvider serviceProvider,
    IOptions<AccessTokenProviderFactoryOptions> options) : IAccessTokenProviderFactory, IDisposable
{
    private readonly
#if NET9_0_OR_GREATER
        Lock
#else
        object
#endif
    _lock = new();
    private readonly Dictionary<string, IAccessTokenProvider> _providers = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Func<IServiceProvider, IAccessTokenProvider>> _factories = options.Value.Factories;
    private bool _disposed;

    /// <summary>
    /// Gets the cached provider registered under <paramref name="name"/>, creating it on first access.
    /// </summary>
    /// <param name="name">The provider name. An empty string selects the default provider.</param>
    /// <returns>The provider instance associated with <paramref name="name"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">No provider is registered under <paramref name="name"/>.</exception>
    /// <exception cref="ObjectDisposedException">This factory has been disposed.</exception>
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

    /// <summary>
    /// Disposes every distinct disposable provider created by this factory.
    /// </summary>
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
