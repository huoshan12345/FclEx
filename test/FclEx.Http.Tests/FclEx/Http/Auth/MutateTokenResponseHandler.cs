namespace FclEx.Http.Auth;

public class MutateTokenResponseHandler : DelegatingHandler
{
    public int TokenRequestCount { get; private set; }
    public string AccessToken { get; set; } = "fake_token";
    public int ExpiresIn { get; set; } = 3600;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        // ReSharper disable once InvertIf
        if (request.RequestUri is { } uri
            && uri.AbsolutePath.StartsWith(TokenPath) 
            && response.IsSuccessStatusCode)
        {
            TokenRequestCount++;

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var jsonNode = JsonNode.Parse(json)!;
            jsonNode[OidcConstants.TokenResponse.ExpiresIn] = ExpiresIn;
            response.Content = HttpContent.Json(jsonNode);
        }

        return response;
    }
}