namespace FclEx.Http;

public class AuthenticationHandler : DelegatingHandler
{
    private readonly string[] _scopes;
    private readonly bool _requireToken;
    private readonly IAccessTokenProvider _tokenProvider;

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

        var token = await _tokenProvider.GetTokenAsync(_scopes);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Unauthorized)
            return response;

        response.Dispose();

        var newToken = await _tokenProvider.GetTokenAsync(_scopes, forceRefresh: true);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
        return await base.SendAsync(request, cancellationToken);
    }
}