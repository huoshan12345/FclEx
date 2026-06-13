namespace FclEx.Http;

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
    private readonly string[] _scopes;
    private readonly bool _requireToken;
    private readonly IAccessTokenProvider _tokenProvider;

    /// <summary>
    /// Initializes a handler that can attach bearer tokens to outgoing requests.
    /// </summary>
    /// <param name="tokenProvider">The provider used to acquire access tokens.</param>
    /// <param name="scopes">The scopes to pass to the provider. <see langword="null"/> is treated as an empty scope list.</param>
    /// <param name="requireToken">
    /// Whether requests should include a bearer token. When <see langword="false"/>, the handler forwards requests without
    /// calling <paramref name="tokenProvider"/>.
    /// </param>
    public AuthenticationHandler(IAccessTokenProvider tokenProvider, string[]? scopes = null, bool requireToken = true)
    {
        _tokenProvider = tokenProvider;
        _scopes = scopes ?? [];
        _requireToken = requireToken;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_requireToken == false)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var token = await _tokenProvider.GetTokenAsync(_scopes, forceRefresh: false, cancellationToken);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Unauthorized)
            return response;

        response.Dispose();

        var newToken = await _tokenProvider.GetTokenAsync(_scopes, forceRefresh: true, cancellationToken);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
        return await base.SendAsync(request, cancellationToken);
    }
}
