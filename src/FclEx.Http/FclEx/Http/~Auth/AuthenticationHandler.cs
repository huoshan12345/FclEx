namespace FclEx.Http;

public class AuthenticationHandlerOptions
{
    /// <summary>
    /// The name of the <see cref="IAccessTokenProvider"/> to use for acquiring tokens.<br/>
    /// This is used when multiple providers are registered in DI.
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
/// <c>forceRefresh: true</c>, and the same request message is sent one more time.
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
