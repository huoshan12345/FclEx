namespace FclEx.Http;

/// <summary>
/// Acquires and caches OAuth/OIDC access tokens using the client-credentials flow.
/// </summary>
/// <remarks>
/// The discovery document is loaded once and shared across requests. Access tokens are cached per joined scope string;
/// concurrent requests for the same scope are serialized so only one token request is made for a missing or expired token.
/// Cached refresh tokens are tried before falling back to a client-credentials token request. Cached access tokens are
/// treated as expired 30 seconds before the server-reported expiration.
/// </remarks>
public class ClientCredentialsTokenProvider : IAccessTokenProvider
{
    private readonly Func<HttpClient> _httpClientFactory;
    private readonly bool _disposeHttpClient;
    private readonly LfuCache<string, SemaphoreSlim> _locks = new(byte.MaxValue);
    private readonly LfuCache<string, TokenCacheItem> _cache = new(byte.MaxValue);
    private readonly SemaphoreSlim _discoveryLock = new(1, 1);
    private readonly ClientCredentialsTokenProviderOptions _options;
    private readonly DiscoveryDocumentRequest _documentRequest;

    private DiscoveryDocumentResponse? _discovery;

    /// <summary>
    /// Initializes a provider that creates an <see cref="HttpClient"/> for discovery and token requests.
    /// </summary>
    /// <param name="options">The client credentials and discovery settings.</param>
    /// <param name="httpClientFactory">Creates clients used for discovery, refresh-token, and client-credentials requests.</param>
    /// <param name="disposeHttpClient">
    /// Whether clients created by <paramref name="httpClientFactory"/> should be disposed after each discovery or token request.
    /// </param>
    public ClientCredentialsTokenProvider(ClientCredentialsTokenProviderOptions options, Func<HttpClient> httpClientFactory, bool disposeHttpClient = true)
    {
        _options = options;
        _httpClientFactory = httpClientFactory;
        _disposeHttpClient = disposeHttpClient;
        _documentRequest = new DiscoveryDocumentRequest
        {
            Address = options.Authority,
            Policy = options.Policy,
        };
    }

    /// <summary>
    /// Initializes a provider that obtains clients from an <see cref="IHttpClientFactory"/>.
    /// </summary>
    /// <remarks>The created clients are not disposed by this provider, following the ownership model of <see cref="IHttpClientFactory"/>.</remarks>
    /// <param name="options">The client credentials and discovery settings.</param>
    /// <param name="httpClientFactory">The factory used to create named clients for token requests.</param>
    public ClientCredentialsTokenProvider(ClientCredentialsTokenProviderOptions options, IHttpClientFactory httpClientFactory)
        : this(options, () => httpClientFactory.CreateClient(nameof(ClientCredentialsTokenProvider)), false)
    {
    }

    /// <summary>
    /// Initializes a provider that reuses a supplied <see cref="HttpClient"/>.
    /// </summary>
    /// <remarks>The supplied client is not disposed by this provider.</remarks>
    /// <param name="options">The client credentials and discovery settings.</param>
    /// <param name="httpClient">The client used for discovery and token requests.</param>
    public ClientCredentialsTokenProvider(ClientCredentialsTokenProviderOptions options, HttpClient httpClient)
    : this(options, () => httpClient, false)
    {
    }

    /// <summary>
    /// Creates the client used by one discovery or token request.
    /// </summary>
    /// <returns>An HTTP client. Ownership depends on the constructor overload used.</returns>
    protected virtual HttpClient CreateClient() => _httpClientFactory();

    private async Task<DiscoveryDocumentResponse> GetDiscoveryAsync(CancellationToken cancellationToken = default)
    {
        if (_discovery != null)
            return _discovery;

        await _discoveryLock.WaitAsync(cancellationToken);
        HttpClient? httpClient = null;
        try
        {
            if (_discovery != null)
                return _discovery;

            httpClient = CreateClient();
            var disco = await httpClient.GetDiscoveryDocumentAsync(_documentRequest, cancellationToken);

            if (disco.IsError)
                throw new Exception(disco.Error);

            _discovery = disco;
            return _discovery;
        }
        finally
        {
            if (_disposeHttpClient)
                httpClient?.Dispose();

            _discoveryLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<string> GetTokenAsync(string[] scopes, bool forceRefresh = false, CancellationToken cancellationToken = default)
    {
        var scope = scopes.Length switch
        {
            0 => "",
            1 => scopes[0],
            _ => scopes.JoinWith(" "),
        };

        var locker = _locks.GetOrAdd(scope, _ => new SemaphoreSlim(1, 1));

        await locker.WaitAsync(cancellationToken);
        HttpClient? httpClient = null;
        try
        {
            if (forceRefresh == false
                && _cache.TryGetValue(scope, out var cached)
                && cached.IsExpired() == false)
            {
                return cached.AccessToken;
            }

            var disco = await GetDiscoveryAsync(cancellationToken);
            if (disco.IsError)
                throw HttpRequestException.From(disco.Error, null, disco.HttpStatusCode);

            httpClient = CreateClient();

            // try refresh_token
            if (forceRefresh == false
                && _cache.TryGetValue(scope, out cached)
                && cached.RefreshToken is { Length: > 0 } refreshToken)
            {
                var refresh = await httpClient.RequestRefreshTokenAsync(new RefreshTokenRequest
                {
                    Address = disco.TokenEndpoint,
                    ClientId = _options.ClientId,
                    ClientSecret = _options.ClientSecret,
                    RefreshToken = refreshToken,
                }, cancellationToken);

                if (refresh.IsError == false)
                {
                    var newItem = CreateCacheItem(refresh);
                    _cache[scope] = newItem;
                    return newItem.AccessToken;
                }
            }

            // fallback: client credentials
            var token = await httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = _options.ClientId,
                ClientSecret = _options.ClientSecret,
                Scope = scope,
            }, cancellationToken);

            if (token.IsError)
                throw HttpRequestException.From(token.ErrorDescription, null, token.HttpStatusCode);

            var item = CreateCacheItem(token);
            _cache[scope] = item;
            return item.AccessToken;
        }
        finally
        {
            if (_disposeHttpClient)
                httpClient?.Dispose();

            locker.Release();
        }
    }

    private static TokenCacheItem CreateCacheItem(TokenResponse response)
    {
        return new TokenCacheItem(
            response.AccessToken!,
            response.RefreshToken,
            DateTime.UtcNow.AddSeconds(response.ExpiresIn - 30)
        );
    }

    private record TokenCacheItem(string AccessToken, string? RefreshToken, DateTime ExpireAt)
    {
        /// <summary>
        /// Returns whether the cached access token has reached the pre-adjusted expiration time.
        /// </summary>
        public bool IsExpired() => DateTime.UtcNow >= ExpireAt;
    }
}
