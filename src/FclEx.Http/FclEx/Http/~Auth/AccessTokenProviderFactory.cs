namespace FclEx.Http;

public sealed class AccessTokenProviderFactoryOptions
{
    public Dictionary<string, Func<IServiceProvider, IAccessTokenProvider>> Factories { get; } = new(StringComparer.Ordinal);
}

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
