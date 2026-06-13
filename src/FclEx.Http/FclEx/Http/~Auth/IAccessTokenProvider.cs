namespace FclEx.Http;

/// <summary>
/// Supplies bearer access tokens for HTTP requests.
/// </summary>
public interface IAccessTokenProvider
{
    /// <summary>
    /// Gets an access token for the requested scopes.
    /// </summary>
    /// <param name="scopes">The scopes to request. Implementations may treat the order as significant.</param>
    /// <param name="forceRefresh">
    /// Whether to bypass any usable cached token and request a new token. Implementations that do not cache can ignore this value.
    /// </param>
    /// <param name="cancellationToken">A token that cancels token acquisition.</param>
    /// <returns>The access token string without the <c>Bearer</c> scheme prefix.</returns>
    Task<string> GetTokenAsync(string[] scopes, bool forceRefresh = false, CancellationToken cancellationToken = default);
}

/// <summary>
/// Convenience overloads for <see cref="IAccessTokenProvider"/>.
/// </summary>
public static class AccessTokenProviderExtensions
{
    /// <summary>
    /// Gets an access token for a single scope.
    /// </summary>
    /// <param name="provider">The token provider.</param>
    /// <param name="scope">The scope to request.</param>
    /// <param name="forceRefresh">Whether to bypass any usable cached token and request a new token.</param>
    /// <param name="cancellationToken">A token that cancels token acquisition.</param>
    /// <returns>The access token string without the <c>Bearer</c> scheme prefix.</returns>
    public static Task<string> GetTokenAsync(this IAccessTokenProvider provider, string scope, bool forceRefresh = false, CancellationToken cancellationToken = default)
    {
        return provider.GetTokenAsync([scope], forceRefresh, cancellationToken);
    }
}
