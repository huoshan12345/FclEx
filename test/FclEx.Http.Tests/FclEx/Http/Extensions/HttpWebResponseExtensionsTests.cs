namespace FclEx.Http.Extensions;

#if NET8_0_OR_GREATER
#pragma warning disable SYSLIB0014 // These tests cover HttpWebRequest/HttpWebResponse extension methods.
[Collection(nameof(HttpServerTestsCollection))]
public class HttpWebResponseExtensionsTests
{
    [Fact]
    public void GetHttpResponse_WhenServerReturnsFailureStatus_ReturnsErrorResponseInsteadOfThrowingWebException()
    {
        var request = CreateRequest("/api/not-found");

        using var response = request.GetHttpResponse();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetHttpResponseAsync_WhenServerReturnsFailureStatus_ReturnsErrorResponseInsteadOfThrowingWebException()
    {
        var request = CreateRequest("/api/not-found");

        using var response = await request.GetHttpResponseAsync();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public void EnsureSuccess_WhenResponseIsSuccessful_ReturnsSameResponse()
    {
        var request = CreateRequest(TestApiPaths.Discovery);

        using var response = request.GetHttpResponse();
        var actual = response.EnsureSuccess();

        Assert.Same(response, actual);
    }

    [Fact]
    public void EnsureSuccess_WhenResponseIsFailure_ThrowsWithStatusAndRequestContext()
    {
        var request = CreateRequest("/api/not-found");

        using var response = request.GetHttpResponse();
        var ex = Assert.Throws<HttpRequestException>(() => response.EnsureSuccess());

        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
        Assert.Contains("NotFound/404", ex.Message);
        Assert.Contains("GET ", ex.Message);
    }

    [Fact]
    public void IsRedirection_WhenResponseHasRedirectStatusAndLocation_ReturnsTrue()
    {
        var request = CreateRequest($"{TestApiPaths.Redirect}?u=/redirect-target");
        request.AllowAutoRedirect = false;

        using var response = request.GetHttpResponse();

        Assert.True(response.IsRedirection());
    }

    [Fact]
    public void IsRedirection_WhenResponseIsNotRedirect_ReturnsFalse()
    {
        var request = CreateRequest(TestApiPaths.Discovery);

        using var response = request.GetHttpResponse();

        Assert.False(response.IsRedirection());
    }

    [Fact]
    public void GetRedirectUri_WhenLocationIsRelative_ResolvesAgainstResponseUri()
    {
        var request = CreateRequest($"{TestApiPaths.Redirect}?u=/redirect-target?q=1");
        request.AllowAutoRedirect = false;

        using var response = request.GetHttpResponse();
        var uri = response.GetRedirectUri();

        Assert.Equal(new Uri(TestUri, "/redirect-target?q=1"), uri);
    }

    [Fact]
    public void GetRedirectUri_WhenLocationIsMissing_ThrowsArgumentNullException()
    {
        var request = CreateRequest(TestApiPaths.Discovery);

        using var response = request.GetHttpResponse();
        var ex = Assert.Throws<ArgumentNullException>(() => response.GetRedirectUri());

        Assert.Equal(nameof(HttpResponseHeader.Location), ex.ParamName);
    }

    private static HttpWebRequest CreateRequest(string relativeUrl)
    {
        var uri = new Uri(TestUri, relativeUrl);
        return WebRequest.CreateHttp(uri);
    }
}
#pragma warning restore SYSLIB0014
#endif
