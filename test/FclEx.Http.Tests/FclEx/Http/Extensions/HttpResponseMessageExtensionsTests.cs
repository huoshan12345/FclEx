namespace FclEx.Http.Extensions;

public class HttpResponseMessageExtensionsTests
{
    [Fact]
    public void TryGetRedirection_WhenLocationIsAbsolute_ReturnsLocation()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.Found);
        var location = new Uri("https://example.com/next");
        response.Headers.Location = location;

        var found = response.TryGetRedirection(out var uri);

        Assert.True(found);
        Assert.Equal(location, uri);
    }

    [Fact]
    public void TryGetRedirection_WhenLocationIsRelativeAndRequestUriExists_ResolvesAgainstRequestUri()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/root/page");
        using var response = new HttpResponseMessage(HttpStatusCode.MovedPermanently)
        {
            RequestMessage = request,
        };
        response.Headers.Location = new Uri("../next?q=1", UriKind.Relative);

        var found = response.TryGetRedirection(out var uri);

        Assert.True(found);
        Assert.Equal(new Uri("https://example.com/next?q=1"), uri);
    }

    [Fact]
    public void TryGetRedirection_WhenStatusCodeIsNotRedirection_ReturnsFalse()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK);
        response.Headers.Location = new Uri("https://example.com/next");

        var found = response.TryGetRedirection(out var uri);

        Assert.False(found);
        Assert.Null(uri);
    }

    [Fact]
    public void TryGetRedirection_WhenLocationIsRelativeWithoutRequestUri_ReturnsFalse()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.Redirect);
        response.Headers.Location = new Uri("/next", UriKind.Relative);

        var found = response.TryGetRedirection(out var uri);

        Assert.False(found);
        Assert.Null(uri);
    }

    [Fact]
    public void EnsureSuccess_WhenResponseStatusCodeIsSuccess_ReturnsResponse()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.Accepted);

        var result = response.EnsureSuccess();

        Assert.Same(response, result);
    }

    [Fact]
    public void EnsureSuccess_WhenResponseStatusCodeIsFailure_ThrowsWithRequestContext()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            RequestMessage = new HttpRequestMessage(HttpMethod.Delete, "https://example.com/items/1"),
        };

        var ex = Assert.Throws<HttpRequestException>(() => response.EnsureSuccess());

        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
        Assert.Contains("NotFound/404", ex.Message);
        Assert.Contains("DELETE https://example.com/items/1", ex.Message);
    }

    [Fact]
    public void TryGetCookies_WhenSetCookieHeadersExist_ReturnsHeaderValues()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK);
        response.Headers.TryAddWithoutValidation(HttpHeaderNames.SetCookie, ["sid=abc", "theme=dark"]);

        var found = response.TryGetCookies(out var cookies);

        Assert.True(found);
        Assert.Equal(["sid=abc", "theme=dark"], cookies);
    }

    [Fact]
    public void TryGetCookies_WhenSetCookieHeaderIsMissing_ReturnsFalse()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK);

        var found = response.TryGetCookies(out var cookies);

        Assert.False(found);
        Assert.Null(cookies);
    }
}
