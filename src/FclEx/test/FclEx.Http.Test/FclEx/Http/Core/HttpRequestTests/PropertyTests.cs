

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
    public async Task DetectChar_Test(bool value)
    {
        using var http = new HttpClientService();
        var req = HttpRequest.Get(CharSetTestCase.Url);
        req.DetectChar(value);

        var res = await req.SendAsync(http)
            .ThrowIfError()
            .IgnoreSyncContext();
        Assert.Equal(value, res.ResponseString.Contains(CharSetTestCase.Keyword));
    }

    public static IEnumerable<object[]> CompressionMethods = Enum.GetValues<CompressionMethod>().Select(m => new object[] { m });

    [Theory]
    [MemberData(nameof(CompressionMethods))]
    public async Task Compress_Test(CompressionMethod compression)
    {
        if (compression is CompressionMethod.Brotli or CompressionMethod.Deflate)
            return; // fastmock 不支持

        var random = new Random(1024);
        var expected = Enumerable.Range(1, 100).ToDictionary(m => m.ToString(), m => random.NextString(5));
        var res = await HttpRequest.Post("api/compress")
            .AddData(expected!)
            .ReadHeadersTimeout(TimeSpan.FromSeconds(30))
            .Compression(compression)
            .SendAsync(TestHttp)
            .ThrowIfError()
            .IgnoreSyncContext();

        Assert.False(res.HasError);

        var token = res.ResponseString.ToJToken();

        var headers = token["headers"]?.ToString();
        Assert.NotNull(headers);

        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        JsonConvert.PopulateObject(headers, result);

        var encoding = result.Get(HttpKnownHeaderNames.ContentEncoding);
        var length = result.Get(HttpKnownHeaderNames.ContentLength, m => int.Parse(m));

        var (expectedEncoding, expectedLength) = compression switch
        {
            CompressionMethod.None => (null, 891),
            CompressionMethod.GZip => ("gzip", 666),
            CompressionMethod.Deflate => ("deflate", 891),
            CompressionMethod.Brotli => ("br", 891),
            _ => throw new ArgumentOutOfRangeException(nameof(compression), compression, null)
        };

        Assert.Equal(expectedEncoding, encoding);
        Assert.Equal(expectedLength, length);

        var body = token["body"];
        Assert.NotNull(body);
        var actual = body.ToObject<Dictionary<string, string>>();
        Assert.Equal(expected, actual);
    }
}