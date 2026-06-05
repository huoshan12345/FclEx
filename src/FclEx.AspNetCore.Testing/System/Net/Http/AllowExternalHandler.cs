using System.Threading;
using System.Threading.Tasks;
using FclEx.Http;

namespace System.Net.Http;

public class AllowExternalHandler(bool allowAutoRedirect = true) : DelegatingHandler
{
    private readonly HttpClient _httpClient = HttpClientHelper.Create();
    private const int MaxRedirects = 10;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var redirects = 0;
        while (true)
        {
            var task = request.RequestUri is { Host: "localhost" }
                ? base.SendAsync(request, cancellationToken)
                : _httpClient.SendAsync(request.SetNotSend(), cancellationToken);

            var response = await task;

            if (allowAutoRedirect == false
                || response.StatusCode.IsRedirection() == false
                || response.Headers.Location is not { } uri
                || redirects++ > MaxRedirects)
                return response;

            request = new HttpRequestMessage(HttpMethod.Get, uri);
        }
    }
}