namespace FclEx.Http;

public class ClientCredentialsTokenProvider : IAccessTokenProvider
{
    private readonly Func<HttpClient> _httpClientFactory;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();
    private readonly ConcurrentDictionary<string, TokenCacheItem> _cache = new();
    private readonly SemaphoreSlim _discoveryLock = new(1, 1);
    private readonly ClientCredentialsTokenProviderOptions _options;
    private readonly DiscoveryDocumentRequest _documentRequest;

    private DiscoveryDocumentResponse? _discovery;

    public ClientCredentialsTokenProvider(Func<HttpClient> httpClientFactory, ClientCredentialsTokenProviderOptions options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
        _documentRequest = new DiscoveryDocumentRequest
        {
            Address = options.Authority,
            Policy = options.Policy,
        };
    }

    public ClientCredentialsTokenProvider(IHttpClientFactory httpClientFactory, ClientCredentialsTokenProviderOptions options)
        : this(() => httpClientFactory.CreateClient(nameof(ClientCredentialsTokenProvider)), options)
    {
    }

    protected virtual HttpClient CreateClient() => _httpClientFactory();

    private async Task<DiscoveryDocumentResponse> GetDiscoveryAsync(CancellationToken cancellationToken = default)
    {
        if (_discovery != null)
            return _discovery;

        await _discoveryLock.WaitAsync(cancellationToken);
        try
        {
            if (_discovery != null)
                return _discovery;

            var disco = await CreateClient().GetDiscoveryDocumentAsync(_documentRequest, cancellationToken);

            if (disco.IsError)
                throw new Exception(disco.Error);

            _discovery = disco;
            return _discovery;
        }
        finally
        {
            _discoveryLock.Release();
        }
    }

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
        try
        {
            if (forceRefresh == false
                && _cache.TryGetValue(scope, out var cached)
                && cached.IsExpired() == false)
            {
                return cached.AccessToken;
            }

            var httpClient = CreateClient();
            var disco = await GetDiscoveryAsync(cancellationToken);

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
        public bool IsExpired() => DateTime.UtcNow >= ExpireAt;
    }
}
