namespace FclEx.Http.Services;

public partial class HttpClientServiceTests
{
    [Fact]
    public async Task SendAsync_WhenResponseHasCookiesButNoRequestMessage_DoesNotRecordNullUri()
    {
        var handler = new ResponseWithoutRequestMessageHandler();
        using var service = HttpClientService.Create(
            () => new HttpClient(handler),
            disposeHttpClient: true,
            options: new()
            {
                RetryCount = 0,
            });

        var response = await HttpRequest.Get("https://example.com/api")
            .SendAsync(service);

        Assert.False(response.IsError, response.Exception?.ToString());
        Assert.Empty(response.VisitedUris);
        Assert.True(response.Headers.TryGetValue(HttpHeaderNames.SetCookie, out var cookies));
        Assert.Equal(["sid=abc; path=/"], cookies);
    }

    private sealed class ResponseWithoutRequestMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("ok"),
                RequestMessage = null,
            };
            response.Headers.Add(HttpHeaderNames.SetCookie, "sid=abc; path=/");
            return Task.FromResult(response);
        }
    }
}
