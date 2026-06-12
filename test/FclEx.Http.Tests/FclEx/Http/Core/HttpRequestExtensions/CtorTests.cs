namespace FclEx.Http.Core.HttpRequestExtensions;

public class CtorTests
{
    public static readonly string[] Urls =
    [
        "https://www.cnblogs.com/armfly/p/9378170.html",
        "/parent/change-old-passwd",
    ];

    public static readonly HttpMethod[] Methods =
    [
        HttpMethod.Get,
        HttpMethod.Post,
        HttpMethod.Put,
        HttpMethod.Delete,
        HttpMethod.Head,
        HttpMethod.Options,
    ];

    public static readonly TheoryData<string, HttpMethod> CtorCases = Urls.CrossJoin(Methods).ToTheoryData();

    [Fact]
    public void Constructor_InitializesDefaultRequestState()
    {
        var uri = new Uri("https://example.com/api");

        var request = new HttpRequest(uri, HttpMethod.Post);

        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Null(request.Content);
        Assert.Equal(HttpVersion.Version11, request.Version);
#if NET6_0_OR_GREATER
        Assert.Equal(HttpVersionPolicy.RequestVersionOrLower, request.VersionPolicy);
#endif
        Assert.Null(request.BufferSize);
        Assert.Equal(TimeSpan.FromMinutes(2), request.TotalTimeout);
        Assert.Equal(TimeSpan.FromSeconds(10), request.ReadBufferTimeout);
        Assert.Equal(TimeSpan.FromSeconds(10), request.ReadHeadersTimeout);
        Assert.Null(request.MediaType);
        Assert.Null(request.CharSet);
        Assert.False(request.DetectCharSet);
        Assert.Null(request.FallbackCharSet);
        Assert.True(request.IgnoreInvalidCharSet);
        Assert.Equal(CompressionMethod.None, request.CompressionMethod);
        Assert.Equal(CompressionLevel.NoCompression, request.CompressionLevel);
        Assert.Equal(HttpContentType.String, request.ResponseContentType);
        Assert.True(request.ReadContent);
        Assert.True(request.ReadCookies);
        Assert.Equal(50, request.MaxRedirectCount);
        Assert.True(request.AllowInsecureRedirects);
        Assert.True(request.UseDefaultUserAgent);
        Assert.False(request.AddHeaderWithoutValidation);
        Assert.Empty(request.Headers);
        Assert.Empty(request.Query);
        Assert.Empty(request.Form);
        Assert.Equal(uri, request.GetUri());
    }

    [Theory]
    [MemberData(nameof(CtorCases))]
    public void Create_WithUriAndMethod_CreatesRequest(string url, HttpMethod method)
    {
        var uri = new Uri(url, UriKind.RelativeOrAbsolute);

        var request = HttpRequest.Create(uri, method);

        Assert.Equal(method, request.Method);
        Assert.Equal(uri, request.GetUri());
    }

    [Theory]
    [MemberData(nameof(CtorCases))]
    public void Create_WithStringAndMethod_CreatesRequest(string url, HttpMethod method)
    {
        var request = HttpRequest.Create(url, method);

        Assert.Equal(method, request.Method);
        Assert.Equal(new Uri(url, UriKind.RelativeOrAbsolute), request.GetUri());
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    [InlineData("HEAD")]
    [InlineData("OPTIONS")]
    public void NamedFactory_WithStringUri_CreatesRequestWithExpectedMethod(string method)
    {
        var request = method switch
        {
            "GET" => HttpRequest.Get("https://example.com/api"),
            "POST" => HttpRequest.Post("https://example.com/api"),
            "PUT" => HttpRequest.Put("https://example.com/api"),
            "DELETE" => HttpRequest.Delete("https://example.com/api"),
            "HEAD" => HttpRequest.Head("https://example.com/api"),
            "OPTIONS" => HttpRequest.Options("https://example.com/api"),
            _ => throw new ArgumentOutOfRangeException(nameof(method), method, null),
        };

        Assert.Equal(new HttpMethod(method), request.Method);
        Assert.Equal(new Uri("https://example.com/api"), request.GetUri());
    }

    [Theory]
    [MemberData(nameof(CtorCases))]
    public void TestCtor(string url, HttpMethod method)
    {
        var request = new HttpRequest(new Uri(url, UriKind.RelativeOrAbsolute), method);
        request.Host("localhost");
        var realUrl = request.GetUri();
    }

    [Fact]
    public void Ctor_WithUserInfo()
    {
        var request = new HttpRequest(new Uri("http://tom:tom123@localhost/api/save"), HttpMethod.Get);
        Assert.Equal("tom", request.UserName);
        Assert.Equal("tom123", request.Password);

        request.BasicAuth(request.UserName, request.Password);

        Assert.True(request.Headers.TryGet(HttpHeaderNames.Authorization, out var auth));
        Assert.Equal("Basic dG9tOnRvbTEyMw==", auth);
    }
}
