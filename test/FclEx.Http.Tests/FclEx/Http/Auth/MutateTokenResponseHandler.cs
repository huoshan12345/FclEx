namespace FclEx.Http.Auth;

public delegate void MutateTokenResponse(
    MutateTokenResponseHandler handler,
    HttpRequestMessage request,
    HttpResponseMessage response,
    JsonNode responseJson);

public class MutateTokenResponseHandler : DelegatingHandler
{
    public int TokenRequestCount { get; private set; }

    private readonly MutateTokenResponse? _action;

    public MutateTokenResponseHandler(MutateTokenResponse? action = null)
    {
        _action = action;
    }

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

            // ReSharper disable once InvertIf
            if (_action is not null)
            {
                _action.Invoke(this, request, response, jsonNode);
                response.Content = HttpContent.Json(jsonNode);
            }
        }

        return response;
    }
}