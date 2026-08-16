using FclEx.Http.Models;

namespace FclEx.Http.Core.HttpRequestExtensions;

public class PropertyTests : HttpServerTests
{
    public static readonly (string Url, string TestUrl, string CharSet, string Keyword) CharSetTestCase
        = ("https://passport.weibo.com/visitor/visitor", TestApiPaths.CharsetDetectGb2312, "gb2312", "是否采集设备指纹");

    [Fact]
    public void BasicAuth_EncodesUserNameAndPasswordAsBasicAuthorizationHeader()
    {
        var request = HttpRequest.Get("http://localhost");

        var result = request.BasicAuth("alice", "p@ss");

        Assert.Same(request, result);
        Assert.Equal("Basic YWxpY2U6cEBzcw==", request.Authorization);
    }

    [Fact]
    public void BearerAuth_SetsBearerAuthorizationHeader()
    {
        var request = HttpRequest.Get("http://localhost");

        var result = request.BearerAuth("token");

        Assert.Same(request, result);
        Assert.Equal("Bearer token", request.Authorization);
    }

    [Fact]
    public void FluentBooleanAndValueProperties_SetRequestPropertiesAndReturnRequest()
    {
        var request = HttpRequest.Get("http://localhost");
        var version = new Version(2, 0);

        var result = request
            .EnsureSuccessStatusCode()
            .ReadContent(false)
            .ReadCookies(false)
            .UseDefaultUserAgent(false)
            .AddHeaderWithoutValidation(true)
            .AllowInsecureRedirects(false)
            .MaxRedirectCount(3)
            .BufferSize(4096)
            .ReadHeadersTimeout(TimeSpan.FromSeconds(1))
            .ReadBufferTimeout(TimeSpan.FromSeconds(2))
            .TotalTimeout(TimeSpan.FromSeconds(3))
            .ReadAsBytes()
            .Method(HttpMethod.Post)
            .Version(version);

        Assert.Same(request, result);
        Assert.True(request.EnsureSuccessStatusCode);
        Assert.False(request.ReadContent);
        Assert.False(request.ReadCookies);
        Assert.False(request.UseDefaultUserAgent);
        Assert.True(request.AddHeaderWithoutValidation);
        Assert.False(request.AllowInsecureRedirects);
        Assert.Equal(3, request.MaxRedirectCount);
        Assert.Equal(4096, request.BufferSize);
        Assert.Equal(TimeSpan.FromSeconds(1), request.ReadHeadersTimeout);
        Assert.Equal(TimeSpan.FromSeconds(2), request.ReadBufferTimeout);
        Assert.Equal(TimeSpan.FromSeconds(3), request.TotalTimeout);
        Assert.Equal(HttpContentType.Bytes, request.ResponseContentType);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal(version, request.Version);
    }

    [Fact]
    public void FluentStringMethod_SetsCustomHttpMethod()
    {
        var request = HttpRequest.Get("http://localhost");

        var result = request.Method("PATCH");

        Assert.Same(request, result);
        Assert.Equal(new HttpMethod("PATCH"), request.Method);
    }

#if NET6_0_OR_GREATER
    [Fact]
    public void VersionPolicy_SetsRequestVersionPolicy()
    {
        var request = HttpRequest.Get("http://localhost");

        var result = request.VersionPolicy(HttpVersionPolicy.RequestVersionOrHigher);

        Assert.Same(request, result);
        Assert.Equal(HttpVersionPolicy.RequestVersionOrHigher, request.VersionPolicy);
    }
#endif

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DetectCharSet_SetsFlagAndReturnsRequest(bool value)
    {
        var request = HttpRequest.Get("http://localhost");

        var result = request.DetectCharSet(value);

        Assert.Same(request, result);
        Assert.Equal(value, request.DetectCharSet);
    }

    [Fact]
    public void AuthorizationHelpers_OverwriteAndRemoveAuthorizationHeader()
    {
        var request = HttpRequest.Get("http://localhost")
            .BearerAuth("token");

        request.Auth("Custom value");
        Assert.Equal("Custom value", request.Authorization);

        request.Auth(null);
        Assert.Null(request.Authorization);
        Assert.False(request.Headers.ContainsKey(HttpHeaderNames.Authorization));
    }

    [Fact]
    public void TryOrigin_WhenOriginExists_DoesNotOverwriteValue()
    {
        var request = HttpRequest.Get("http://localhost")
            .Origin("https://first.example");

        var result = request.TryOrigin("https://second.example");

        Assert.Same(request, result);
        Assert.Equal("https://first.example", request.Origin);
    }

    [Fact]
    public void TryOrigin_WhenOriginIsMissing_SetsValue()
    {
        var request = HttpRequest.Get("http://localhost");

        var result = request.TryOrigin("https://first.example");

        Assert.Same(request, result);
        Assert.Equal("https://first.example", request.Origin);
    }

    [Theory]
    [InlineData(nameof(global::FclEx.Http.HttpRequestExtensions.ReadAsString), HttpContentType.String)]
    [InlineData(nameof(global::FclEx.Http.HttpRequestExtensions.ReadAsBytes), HttpContentType.Bytes)]
    [InlineData(nameof(global::FclEx.Http.HttpRequestExtensions.ReadAsStream), HttpContentType.Stream)]
    public void ReadAsShortcuts_SetResponseContentType(string methodName, HttpContentType expected)
    {
        var request = HttpRequest.Get("http://localhost");

        _ = methodName switch
        {
            nameof(global::FclEx.Http.HttpRequestExtensions.ReadAsString) => request.ReadAsString(),
            nameof(global::FclEx.Http.HttpRequestExtensions.ReadAsBytes) => request.ReadAsBytes(),
            nameof(global::FclEx.Http.HttpRequestExtensions.ReadAsStream) => request.ReadAsStream(),
            _ => throw new ArgumentOutOfRangeException(nameof(methodName), methodName, null),
        };

        Assert.Equal(expected, request.ResponseContentType);
    }

    [Fact]
    public void TryTimeoutAndBufferMethods_DoNotOverwriteExistingValues()
    {
        var request = HttpRequest.Get("http://localhost")
            .ReadHeadersTimeout(TimeSpan.FromSeconds(1))
            .ReadBufferTimeout(TimeSpan.FromSeconds(2))
            .TotalTimeout(TimeSpan.FromSeconds(3))
            .BufferSize(1024);

        var result = request
            .TryReadHeadersTimeout(TimeSpan.FromSeconds(10))
            .TryReadBufferTimeout(TimeSpan.FromSeconds(20))
            .TryTotalTimeout(TimeSpan.FromSeconds(30))
            .TryBufferSize(2048);

        Assert.Same(request, result);
        Assert.Equal(TimeSpan.FromSeconds(1), request.ReadHeadersTimeout);
        Assert.Equal(TimeSpan.FromSeconds(2), request.ReadBufferTimeout);
        Assert.Equal(TimeSpan.FromSeconds(3), request.TotalTimeout);
        Assert.Equal(1024, request.BufferSize);
    }

    [Fact]
    public void TryTimeoutAndBufferMethods_SetMissingValues()
    {
        var request = HttpRequest.Get("http://localhost")
            .ReadHeadersTimeout(null)
            .ReadBufferTimeout(null)
            .TotalTimeout(null)
            .BufferSize(null);

        var result = request
            .TryReadHeadersTimeout(TimeSpan.FromSeconds(10))
            .TryReadBufferTimeout(TimeSpan.FromSeconds(20))
            .TryTotalTimeout(TimeSpan.FromSeconds(30))
            .TryBufferSize(2048);

        Assert.Same(request, result);
        Assert.Equal(TimeSpan.FromSeconds(10), request.ReadHeadersTimeout);
        Assert.Equal(TimeSpan.FromSeconds(20), request.ReadBufferTimeout);
        Assert.Equal(TimeSpan.FromSeconds(30), request.TotalTimeout);
        Assert.Equal(2048, request.BufferSize);
    }

    [Fact]
    public void UriPropertyMethods_UpdateBuiltUri()
    {
        var request = HttpRequest.Get("http://user:pass@example.com:8080/old?x=1#old");

        var result = request
            .Scheme("https")
            .Host("api.example.com")
            .Port(8443)
            .UserName("alice")
            .Password("secret")
            .Path("/v1/items")
            .Fragment("section");

        Assert.Same(request, result);
        Assert.Equal("https://alice:secret@api.example.com:8443/v1/items?x=1#section", request.GetUri().ToString());
    }

    [LocalOnlyFact]
    public async Task SaveCharSetTestResponseBytes()
    {
        var assemblyName = typeof(PropertyTests).Assembly.GetName().Name;
        Assert.NotNull(assemblyName);
        var dir = Path.ToDirectoryInfo(AppContext.BaseDirectory.TakeUntil(assemblyName), "Resources");
        var file = dir.TryCreate().File("visitor.html");

        if (file.Exists)
            return;

        var (url, _, charset, keyword) = CharSetTestCase;
        var response = await HttpRequest.Get(url)
            .CharSet(charset)
            .SendAsync();

        Assert.Contains(keyword, response.ResponseString);
        await file.WriteAllTextAsync(response.ResponseString);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CharSet_WhenConfigured_DecodesResponseWithSpecifiedEncoding(bool value)
    {
        if (HasApiServer == false)
            return;

        var (_, testUrl, charset, keyword) = CharSetTestCase;

        var request = HttpRequest.Get(testUrl);
        if (value)
            request.CharSet(charset);

        var response = await request
            .SendAsync(TestHttp)
            .ThrowIfError();

        Assert.Equal(value, response.ResponseString.Contains(keyword));
    }

    [Fact]
    public void TryCharSet_WhenCharSetExists_DoesNotOverwriteValue()
    {
        var request = HttpRequest.Get("http://localhost")
            .CharSet("utf-8");

        var result = request.TryCharSet("gb2312");

        Assert.Same(request, result);
        Assert.Equal("utf-8", request.CharSet);
    }

    [Fact]
    public void TryCharSet_WhenCharSetIsMissing_SetsValue()
    {
        var request = HttpRequest.Get("http://localhost");

        var result = request.TryCharSet("gb2312");

        Assert.Same(request, result);
        Assert.Equal("gb2312", request.CharSet);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task FallbackCharSet_WhenConfigured_DecodesResponseWhenHeadersDoNotProvideCharset(bool value)
    {
        if (HasApiServer == false)
            return;

        var (_, testUrl, charset, keyword) = CharSetTestCase;

        var request = HttpRequest.Get(testUrl);
        if (value)
            request.FallbackCharSet(charset);

        var response = await request
            .SendAsync(TestHttp)
            .ThrowIfError();

        Assert.Equal(value, response.ResponseString.Contains(keyword));
    }

    [Fact]
    public void TryFallbackCharSet_WhenFallbackCharSetExists_DoesNotOverwriteValue()
    {
        var request = HttpRequest.Get("http://localhost")
            .FallbackCharSet("utf-8");

        var result = request.TryFallbackCharSet("gb2312");

        Assert.Same(request, result);
        Assert.Equal("utf-8", request.FallbackCharSet);
    }

    [Fact]
    public void TryFallbackCharSet_WhenFallbackCharSetIsMissing_SetsValue()
    {
        var request = HttpRequest.Get("http://localhost");

        var result = request.TryFallbackCharSet("gb2312");

        Assert.Same(request, result);
        Assert.Equal("gb2312", request.FallbackCharSet);
    }

    [Theory]
    [InlineData(nameof(global::FclEx.Http.HttpRequestExtensions.CharSet))]
    [InlineData(nameof(global::FclEx.Http.HttpRequestExtensions.TryCharSet))]
    [InlineData(nameof(global::FclEx.Http.HttpRequestExtensions.FallbackCharSet))]
    [InlineData(nameof(global::FclEx.Http.HttpRequestExtensions.TryFallbackCharSet))]
    public void CharSetMethods_UseCorrectParameterName(string methodName)
    {
        var method = typeof(global::FclEx.Http.HttpRequestExtensions)
            .GetMethods()
            .Single(m => m.Name == methodName && m.GetParameters().Length == 2);

        Assert.Equal("charSet", method.GetParameters()[1].Name);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DetectCharSet_WhenEnabled_DetectsCharsetFromResponseBody(bool value)
    {
        if (HasApiServer == false)
            return;

        var (_, testUrl, _, keyword) = CharSetTestCase;

        var response = await HttpRequest
            .Get(testUrl)
            .DetectCharSet(value)
            .SendAsync(TestHttp)
            .ThrowIfError();

        Assert.Equal(value, response.ResponseString.Contains(keyword));
    }

    public static readonly TheoryData<CompressionMethod> CompressionMethods = Enum.GetValues<CompressionMethod>().ToTheoryData();

    [RetryTheory]
    [MemberData(nameof(CompressionMethods))]
    public async Task Compression_WhenRemoteServerReceivesRequest_RoundTripsCompressedJson(CompressionMethod compression)
    {
        if (HasApiServer == false)
            return;

#if NET6_0_OR_GREATER
        if (compression == CompressionMethod.Brotli) // the website does not support
            return;
#endif

        var random = new Random();
        var model = new MockApiModel
        {
            CreatedAt = DateTimeOffset.UtcNow,
            Name = random.NextString(10),
            Avatar = $"https://cloudflare-ipfs.com/ipfs/{random.NextString(10)}/avatar/{random.Next(10, 99)}.jpg",
            Id = 1,
        };
        var response = await HttpRequest.Put("https://65c333b1f7e6ea59682c21a5.mockapi.io/api/compress/" + model.Id)
            .Compression(compression)
            .JsonContent(model)
            .SendAsync(TestHttp);

        Assert.True(response.StatusCode.IsSuccess(), response.ResponseString);
        Assert.False(response.IsError, response.Exception?.Message);

        var returned = response.ResponseString.FromJson<MockApiModel>();
        Assert.MembersEqual(model, returned);
    }

    [Theory]
    [MemberData(nameof(CompressionMethods))]
    public async Task Compression_WhenLocalServerReceivesRequest_RoundTripsCompressedJson(CompressionMethod compression)
    {
        if (HasApiServer == false)
            return;

        if (compression != CompressionMethod.None && Environment.Version.Major < 7)
            return; // test server in aspnet 6.0 has not configured decompression.

        var random = new Random(1024);
        var expected = Enumerable.Range(1, 100).ToDictionary(m => m.ToString(), m => random.NextString(5));
        var response = await HttpRequest.Post(TestApiPaths.Compress)
            .JsonContent(expected)
            .Compression(compression)
            .SendAsync(TestHttp);

        Assert.True(response.StatusCode.IsSuccess(), response.ResponseString);
        Assert.False(response.IsError, response.Exception?.Message);

        var token = response.ResponseString.ToJsonNode();

        Assert.NotNull(token);

        var headers = token["headers"]?.Deserialize<Dictionary<string, string>>();
        Assert.NotNull(headers);

        var encoding = headers.Get(HttpHeaderNames.ContentEncoding);
        var length = headers.Get(HttpHeaderNames.ContentLength, m => m.ToInt());

        var (expectedEncoding, expectedLength) = compression switch
        {
            CompressionMethod.None => (null, 1293),
            CompressionMethod.GZip => ("gzip", 666),
            CompressionMethod.Deflate => ("deflate", 891),
#if NET6_0_OR_GREATER
            CompressionMethod.Brotli => ("br", 891),
#endif
            _ => throw new ArgumentOutOfRangeException(nameof(compression), compression, null)
        };

        // NOTE: aspnet decompression removes header ContentEncoding and ContentLength, so we don't check them here.
        //Assert.Equal(expectedEncoding, encoding);
        //Assert.Equal(expectedLength, length);

        Assert.Null(encoding);
        Assert.Equal(compression == CompressionMethod.None ? expectedLength : null, length);

        var body = token["body"];
        Assert.NotNull(body);
        var actual = body.Deserialize<Dictionary<string, string>>();
        Assert.Equal(expected, actual);
    }
}
