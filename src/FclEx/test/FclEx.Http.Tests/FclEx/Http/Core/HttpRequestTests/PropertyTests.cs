using System.Text.Json;

namespace FclEx.Http.Core.HttpRequestTests;

public class PropertyTests
{
    public static readonly (string Url, string CharSet, string Keyword) CharSetTestCase = ("https://passport.weibo.com/visitor/visitor", "gb2312", "是否采集设备指纹");

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CharSet_Test(bool value)
    {
        using var http = new HttpClientService();
        var request = HttpRequest.Get(CharSetTestCase.Url);
        if (value)
            request.CharSet(CharSetTestCase.CharSet);

        var res = await request.SendAsync(http)
            .ThrowIfError()
            .IgnoreSyncContext();

        Assert.Equal(value, res.ResponseString.Contains(CharSetTestCase.Keyword));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task FallbackCharSet_Test(bool value)
    {
        using var http = new HttpClientService();
        var request = HttpRequest.Get(CharSetTestCase.Url);
        if (value)
            request.CharSet(CharSetTestCase.CharSet);

        var res = await request.SendAsync(http)
            .ThrowIfError()
            .IgnoreSyncContext();
        Assert.Equal(value, res.ResponseString.Contains(CharSetTestCase.Keyword));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DetectCharSet_Test(bool value)
    {
        using var http = new HttpClientService();
        var req = HttpRequest.Get(CharSetTestCase.Url);
        req.DetectCharSet(value);

        var res = await req.SendAsync(http)
            .ThrowIfError()
            .IgnoreSyncContext();
        Assert.Equal(value, res.ResponseString.Contains(CharSetTestCase.Keyword));
    }

    public static readonly IEnumerable<object[]> CompressionMethods = Enum.GetValues<CompressionMethod>().Select(m => new object[] { m });


    [Theory]
    [MemberData(nameof(CompressionMethods))]
    public async Task Compress_Test(CompressionMethod compression)
    {
        if (compression == CompressionMethod.Brotli) // not supported
            return;

        var random = new Random();
        var model = new MockApiModel
        {
            CreatedAt = DateTimeOffset.UtcNow,
            Name = random.NextString(10),
            Avatar = $"https://cloudflare-ipfs.com/ipfs/{random.NextString(10)}/avatar/{random.Next(10, 99)}.jpg",
            Id = 1,
        };
        var res = await HttpRequest.Put("https://65c333b1f7e6ea59682c21a5.mockapi.io/api/compress/" + model.Id)
            .Compression(compression)
            .JsonContent(model)
            .SendAsync(TestHttp)
            .IgnoreSyncContext();

        Assert.True(res.StatusCode.IsSuccess(), res.ResponseString);
        Assert.False(res.HasError, res.Exception?.Message);

        var returned = res.ResponseString.FromJson<MockApiModel>();
        AssertExt.EveryMemberEqual(model, returned);
    }

    [Theory]
    [MemberData(nameof(CompressionMethods))]
    public async Task Compress_LocalServer_Test(CompressionMethod compression)
    {
        if (compression != CompressionMethod.None && Environment.Version.Major < 7)
            return; // test server in aspnet 6.0 has not configured decompression.

        var random = new Random(1024);
        var expected = Enumerable.Range(1, 100).ToDictionary(m => m.ToString(), m => random.NextString(5));
        var res = await HttpRequest.Post("api/compress")
            .JsonContent(expected)
            .Compression(compression)
            .SendAsync(TestHttp)
            .IgnoreSyncContext();

        Assert.True(res.StatusCode.IsSuccess(), res.ResponseString);
        Assert.False(res.HasError, res.Exception?.Message);

        var token = res.ResponseString.ToJsonNode();

        Assert.NotNull(token);

        var headers = token["headers"]?.Deserialize<Dictionary<string, string>>();
        Assert.NotNull(headers);

        var encoding = headers.Get(HttpKnownHeaderNames.ContentEncoding);
        var length = headers.Get(HttpKnownHeaderNames.ContentLength, m => int.Parse(m));

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