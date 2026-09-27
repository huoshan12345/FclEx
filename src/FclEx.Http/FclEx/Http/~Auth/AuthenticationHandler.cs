namespace FclEx.Http;

/// <summary>
/// Configures which access token provider and scopes an <see cref="AuthenticationHandler"/> uses.
/// </summary>
public class AuthenticationHandlerOptions
{
    /// <summary>
    /// The registered provider name to use for acquiring tokens. An empty string selects the default provider.
    /// </summary>
    public string TokenProviderName { get; set; } = string.Empty;

    /// <summary>
    /// The scopes to pass to the provider.
    /// </summary>
    public string[] Scopes { get; set; } = [];

    /// <summary>
    /// Whether requests should include a bearer token. When <see langword="false"/>, the handler forwards requests without
    /// calling <see cref="IAccessTokenProvider"/>.
    /// </summary>
    public bool RequireToken { get; set; } = true;
}

/// <summary>
/// Adds a bearer token to outgoing requests and retries once with a refreshed token after a 401 response.
/// </summary>
/// <remarks>
/// When token acquisition is enabled, the handler asks the configured <see cref="IAccessTokenProvider"/> for a token,
/// assigns it to <see cref="HttpRequestHeaders.Authorization"/>, and sends the request. If the response status is
/// <see cref="HttpStatusCode.Unauthorized"/>, the response is disposed, a second token is requested with
/// <c>forceRefresh: true</c>, and the same request message is sent one more time. The provider is selected once when the
/// handler is constructed; registration extensions resolve it by <see cref="AuthenticationHandlerOptions.TokenProviderName"/>.
/// </remarks>
public class AuthenticationHandler : DelegatingHandler
{
    private readonly AuthenticationHandlerOptions _options;
    private readonly IAccessTokenProvider _tokenProvider;

    /// <summary>
    /// Initializes a handler that can attach bearer tokens to outgoing requests.
    /// </summary>
    /// <param name="options">The options used to configure the handler.</param>
    /// <param name="tokenProvider">The provider used to acquire access tokens.</param>
    public AuthenticationHandler(AuthenticationHandlerOptions options, IAccessTokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
        _options = options;
    }

    /// <summary>
    /// Initializes a handler that selects its provider from a named provider factory.
    /// </summary>
    /// <param name="options">The provider name, scopes, and token requirement used by the handler.</param>
    /// <param name="tokenProviderFactory">The factory that resolves the provider named in <paramref name="options"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> or <paramref name="tokenProviderFactory"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">No provider is registered under the configured name.</exception>
    public AuthenticationHandler(AuthenticationHandlerOptions options, IAccessTokenProviderFactory tokenProviderFactory)
    {
        _tokenProvider = tokenProviderFactory.GetRequired(options.TokenProviderName);
        _options = options;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_options.RequireToken == false)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var token = await _tokenProvider.GetTokenAsync(_options.Scopes, forceRefresh: false, cancellationToken);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Unauthorized)
            return response;

        response.Dispose();

        var newToken = await _tokenProvider.GetTokenAsync(_options.Scopes, forceRefresh: true, cancellationToken);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
        return await base.SendAsync(request, cancellationToken);
    }
}
