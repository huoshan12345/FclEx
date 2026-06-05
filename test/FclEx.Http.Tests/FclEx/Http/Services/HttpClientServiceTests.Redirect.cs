namespace FclEx.Http.Services;

public partial class HttpClientServiceTests
{
    [Fact]
    public async Task SendAsync_WhenTemporaryRedirectPreservesMethod_ResendsPostBody()
    {
        var handler = new RedirectHandler((request, index) => index == 1
            ? CreateRedirectResponse((HttpStatusCode)307, "/target")
            : CreateOkResponse(request));
        using var service = CreateService(handler);

        var response = await HttpRequest.Post("https://example.com/start")
            .StringContent("payload")
            .SendAsync(service);

        Assert.False(response.IsError, response.Exception?.ToString());
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, handler.Requests.Count);
        Assert.Equal(HttpMethod.Post, handler.Requests[0].Method);
        Assert.Equal(HttpMethod.Post, handler.Requests[1].Method);
        Assert.Equal("payload", handler.Requests[0].Body);
        Assert.Equal("payload", handler.Requests[1].Body);
        Assert.Equal(new Uri("https://example.com/target"), handler.Requests[1].Uri);
    }

    [Fact]
    public async Task SendAsync_WhenSeeOtherRedirectsPost_UsesGetWithoutBody()
    {
        var handler = new RedirectHandler((request, index) => index == 1
            ? CreateRedirectResponse(HttpStatusCode.SeeOther, "/target")
            : CreateOkResponse(request));
        using var service = CreateService(handler);

        var response = await HttpRequest.Post("https://example.com/start")
            .StringContent("payload")
            .SendAsync(service);

        Assert.False(response.IsError, response.Exception?.ToString());
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, handler.Requests.Count);
        Assert.Equal(HttpMethod.Post, handler.Requests[0].Method);
        Assert.Equal(HttpMethod.Get, handler.Requests[1].Method);
        Assert.Equal("payload", handler.Requests[0].Body);
        Assert.Null(handler.Requests[1].Body);
        Assert.Equal(new Uri("https://example.com/target"), handler.Requests[1].Uri);
    }

    [Fact]
    public async Task SendAsync_WhenRedirectTargetWasAlreadyVisited_ReturnsLoopError()
    {
        var handler = new RedirectHandler((request, _) => CreateRedirectResponse(HttpStatusCode.Found, request.RequestUri!));
        using var service = CreateService(handler);

        var response = await HttpRequest.Get("https://example.com/start")
            .SendAsync(service);

        Assert.True(response.IsError);
        Assert.IsType<InvalidOperationException>(response.Exception);
        Assert.Contains("redirect loop", response.Exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task SendAsync_WhenRedirectCountExceedsLimit_ReturnsError()
    {
        var handler = new RedirectHandler((_, index) => index switch
        {
            1 => CreateRedirectResponse(HttpStatusCode.Found, "/first"),
            2 => CreateRedirectResponse(HttpStatusCode.Found, "/second"),
            _ => CreateOkResponse(),
        });
        using var service = CreateService(handler);

        var response = await HttpRequest.Get("https://example.com/start")
            .MaxRedirectCount(1)
            .SendAsync(service);

        Assert.True(response.IsError);
        Assert.IsType<InvalidOperationException>(response.Exception);
        Assert.Contains("maximum number of redirects", response.Exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(2, handler.Requests.Count);
    }

    [Fact]
    public async Task SendAsync_WhenInsecureRedirectIsAllowed_FollowsHttpsToHttpRedirect()
    {
        var handler = new RedirectHandler((request, index) => index == 1
            ? CreateRedirectResponse(HttpStatusCode.Found, "http://example.com/target")
            : CreateOkResponse(request));
        using var service = CreateService(handler);

        var response = await HttpRequest.Get("https://example.com/start")
            .SendAsync(service);

        Assert.False(response.IsError, response.Exception?.ToString());
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, handler.Requests.Count);
        Assert.Equal(new Uri("http://example.com/target"), handler.Requests[1].Uri);
    }

    [Fact]
    public async Task SendAsync_WhenInsecureRedirectIsNotAllowed_DoesNotFollowHttpsToHttpRedirect()
    {
        var handler = new RedirectHandler((_, _) => CreateRedirectResponse(HttpStatusCode.Found, "http://example.com/target"));
        using var service = CreateService(handler);

        var response = await HttpRequest.Get("https://example.com/start")
            .AllowInsecureRedirects(false)
            .SendAsync(service);

        Assert.False(response.IsError, response.Exception?.ToString());
        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task SendAsync_WhenRedirecting_RemovesAuthorizationAndUsesCookiesForRedirectUri()
    {
        var handler = new RedirectHandler((request, index) => index == 1
            ? CreateRedirectResponse(HttpStatusCode.Found, "/target")
            : CreateOkResponse(request));
        using var service = CreateService(handler, useCookie: true);
        service.AddCookie(new Cookie("target", "cookie", "/target"), new Uri("https://example.com/target"));

        var response = await HttpRequest.Get("https://example.com/start")
            .BearerAuth("token")
            .SetCookies("source=cookie")
            .SendAsync(service);

        Assert.False(response.IsError, response.Exception?.ToString());
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, handler.Requests.Count);
        Assert.Equal("Bearer token", handler.Requests[0].Authorization);
        Assert.Equal("source=cookie", handler.Requests[0].Cookie);
        Assert.Null(handler.Requests[1].Authorization);
        Assert.Equal("target=cookie", handler.Requests[1].Cookie);
    }

    [Fact]
    public async Task SendAsync_WhenRedirecting_DropsTargetSpecificHeaders()
    {
        var handler = new RedirectHandler((_, index) => index == 1
            ? CreateRedirectResponse(HttpStatusCode.Found, "https://other.example.com/target")
            : CreateOkResponse());
        using var service = CreateService(handler);

        var response = await HttpRequest.Get("https://example.com/start")
            .BearerAuth("token")
            .SetCookies("source=cookie")
            .Origin("https://origin.example.com")
            .Referrer("https://example.com/referrer")
            .SetHeader(HttpHeaderNames.Host, "example.com")
            .SendAsync(service);

        Assert.False(response.IsError, response.Exception?.ToString());
        Assert.Equal(2, handler.Requests.Count);
        Assert.Equal("Bearer token", handler.Requests[0].Authorization);
        Assert.Equal("source=cookie", handler.Requests[0].Cookie);
        Assert.Equal("https://origin.example.com", handler.Requests[0].Origin);
        Assert.Equal("https://example.com/referrer", handler.Requests[0].Referrer);
        Assert.Equal("example.com", handler.Requests[0].Host);
        Assert.Null(handler.Requests[1].Authorization);
        Assert.Null(handler.Requests[1].Cookie);
        Assert.Null(handler.Requests[1].Origin);
        Assert.Null(handler.Requests[1].Referrer);
        Assert.Null(handler.Requests[1].Host);
    }

    private static HttpClientService CreateService(HttpMessageHandler handler, bool useCookie = false)
    {
        return HttpClientService.Create(
            () => new HttpClient(handler),
            disposeHttpClient: true,
            options: new()
            {
                RetryPolicyOptions = new()
                {
                    RetryCount = 0,
                },
            },
            useCookie: useCookie);
    }

    private static HttpResponseMessage CreateRedirectResponse(HttpStatusCode statusCode, string location)
    {
        return CreateRedirectResponse(statusCode, new Uri(location, UriKind.RelativeOrAbsolute));
    }

    private static HttpResponseMessage CreateRedirectResponse(HttpStatusCode statusCode, Uri location)
    {
        var response = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(string.Empty),
        };
        response.Headers.Location = location;
        return response;
    }

    private static HttpResponseMessage CreateOkResponse(HttpRequestMessage? request = null)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            RequestMessage = request,
            Content = new StringContent("ok"),
        };
    }

    private sealed class RedirectHandler(Func<HttpRequestMessage, int, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        public List<SentRequest> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);
            var cookie = request.Headers.TryGetValues(HttpHeaderNames.Cookie, out var cookies)
                ? cookies.JoinWith("; ")
                : null;
            Requests.Add(new(
                request.Method,
                request.RequestUri!,
                body,
                request.Headers.Authorization?.ToString(),
                cookie,
                GetHeader(request, HttpHeaderNames.Origin),
                GetHeader(request, HttpHeaderNames.Referrer),
                request.Headers.Host));

            var response = responseFactory(request, Requests.Count);
            response.RequestMessage ??= request;
            return response;
        }

        private static string? GetHeader(HttpRequestMessage request, string name)
        {
            return request.Headers.TryGetValues(name, out var values)
                ? values.FirstOrDefault()
                : null;
        }
    }

    private readonly record struct SentRequest(
        HttpMethod Method,
        Uri Uri,
        string? Body,
        string? Authorization,
        string? Cookie,
        string? Origin,
        string? Referrer,
        string? Host);
}
