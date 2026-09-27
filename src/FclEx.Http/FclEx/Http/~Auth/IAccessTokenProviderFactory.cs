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