namespace FclEx.Http.Services;

public partial class HttpClientServiceTests
{
    [Fact]
    public async Task BuildHttpRequest_WhenBuiltAgainAfterFirstMessageIsDisposed_CanStillSerializeContent()
    {
        var request = HttpRequest.Post("https://example.com/api")
            .StringContent("payload");

        using (var firstMessage = TestHttpClientService.BuildHttpRequest(request))
        {
            var content = await firstMessage.Content!.ReadAsStringAsync();
            Assert.Equal("payload", content);
        }

        using var secondMessage = TestHttpClientService.BuildHttpRequest(request);
        var secondContent = await secondMessage.Content!.ReadAsStringAsync();

        Assert.Equal("payload", secondContent);
    }

    [Fact]
    public async Task SendAsync_WhenOuterRetryRebuildsRequest_CanResendContent()
    {
        var handler = new CancelOnceThenOkHandler();
        using var service = HttpClientService.Create(
            () => new HttpClient(handler),
            disposeHttpClient: true,
            options: new()
            {
                RetryCount = 1,
                SleepDurationProvider = _ => TimeSpan.Zero,
            },
            useCookie: false);

        var response = await HttpRequest.Post("https://example.com/api")
            .StringContent("payload")
            .SendAsync(service);

        Assert.False(response.IsError, response.Exception?.ToString());
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(new[] { "payload", "payload" }, handler.RequestContents);
    }

    private sealed class TestHttpClientService : HttpClientService
    {
        public static HttpRequestMessage BuildHttpRequest(HttpRequest request)
        {
            return BuildHttpRequest(request, null, new CookieContainer(), CancellationToken.None);
        }
    }

    private sealed class CancelOnceThenOkHandler : HttpMessageHandler
    {
        public List<string> RequestContents { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestContents.Add(await request.Content!.ReadAsStringAsync(cancellationToken));

            if (RequestContents.Count == 1)
                throw new TaskCanceledException(Task.CompletedTask);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                RequestMessage = request,
                Content = new StringContent("ok"),
            };
        }
    }
}
