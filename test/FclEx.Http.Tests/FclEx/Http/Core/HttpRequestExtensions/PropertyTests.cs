namespace FclEx.Http.Core.HttpRequestExtensions;

public class PropertyTests : HttpServerTests
{
    public static readonly (string Url, string TestUrl, string CharSet, string Keyword) CharSetTestCase
        = ("https://passport.weibo.com/visitor/visitor", "/api/charset-detect/gb2312", "gb2312", "是否采集设备指纹");

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
    public async Task CharSet_Test(bool value)
    {
        var (_, testUrl, charset, keyword) = CharSetTestCase;

        var request = HttpRequest.Get(testUrl);
        if (value)
            request.CharSet(charset);

        var response = await request
            .SendAsync(TestHttp)
            .ThrowIfError();

        Assert.Equal(value, response.ResponseString.Contains(keyword));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task FallbackCharSet_Test(bool value)
    {
        var (_, testUrl, charset, keyword) = CharSetTestCase;

        var request = HttpRequest.Get(testUrl);
        if (value)
            request.FallbackCharSet(charset);

        var response = await request
            .SendAsync(TestHttp)
            .ThrowIfError();

        Assert.Equal(value, response.ResponseString.Contains(keyword));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DetectCharSet_Test(bool value)
    {
        var (_, testUrl, _, keyword) = CharSetTestCase;

        var response = await HttpRequest
            .Get(testUrl)
            .DetectCharSet(value)
            .SendAsync(TestHttp)
            .ThrowIfError();

        Assert.Equal(value, response.ResponseString.Contains(keyword));
    }

    public static readonly TheoryData<CompressionMethod> CompressionMethods = EnumHelper.GetValues<CompressionMethod>().ToTheoryData();

    [RetryTheory]
    [MemberData(nameof(CompressionMethods))]
    public async Task Compress_Test(CompressionMethod compression)
    {
        if (compression == CompressionMethod.Brotli) // the website does not support
            return;

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
    public async Task Compress_LocalServer_Test(CompressionMethod compression)
    {
        if (compression != CompressionMethod.None && Environment.Version.Major < 7)
            return; // test server in aspnet 6.0 has not configured decompression.

        var random = new Random(1024);
        var expected = Enumerable.Range(1, 100).ToDictionary(m => m.ToString(), m => random.NextString(5));
        var response = await HttpRequest.Post("api/compress")
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
        var length = headers.Get(HttpHeaderNames.ContentLength, int.Parse);

        var (expectedEncoding, expectedLength) = compression switch
        {
            CompressionMethod.None => (null, 1293),
            CompressionMethod.GZip => ("gzip", 666),
            CompressionMethod.Deflate => ("deflate", 891),
            CompressionMethod.Brotli => ("br", 891),
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