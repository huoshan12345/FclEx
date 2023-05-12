namespace FclEx.Http.Core.HttpReqTests;

public class PropertyTests
{
    public static (string Url, string CharSet, string Keyword) CharSetTestCase = ("https://passport.weibo.com/visitor/visitor", "gb2312", "是否采集设备指纹");

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CharSet_Test(bool setProp)
    {
        using var http = new HttpClientService();
        var req = HttpReq.Get(CharSetTestCase.Url);
        if (setProp)
            req.CharSet(CharSetTestCase.CharSet);

        var res = await http.SendAsync(req)
            .ThrowIfError()
            .DonotCapture();
        Assert.Equal(setProp, res.ResponseString.Contains(CharSetTestCase.Keyword));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task FallbackCharSet_Test(bool setProp)
    {
        using var http = new HttpClientService();
        var req = HttpReq.Get(CharSetTestCase.Url);
        if (setProp)
            req.CharSet(CharSetTestCase.CharSet);

        var res = await http.SendAsync(req)
            .ThrowIfError()
            .DonotCapture();
        Assert.Equal(setProp, res.ResponseString.Contains(CharSetTestCase.Keyword));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DetectCharSetFromHtmlMeta_Test(bool setProp)
    {
        using var http = new HttpClientService();
        var req = HttpReq.Get(CharSetTestCase.Url);
        req.DetectCharSetFromHtmlMeta(setProp);

        var res = await http.SendAsync(req)
            .ThrowIfError()
            .DonotCapture();
        Assert.Equal(setProp, res.ResponseString.Contains(CharSetTestCase.Keyword));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GZip_Test(bool setProp)
    {
        var random = new Random(1024);
        var expected = Enumerable.Range(1, 3).ToDictionary(m => m.ToString(), m => random.NextString(5));
        var res = await HttpReq.Form(UrlUtil.Combine(GlobalConstants.TestUrl, "/api/gzip"))
            .AddData(expected)
            .ConnectTimeout(TimeSpan.FromSeconds(30))
            .GZip(setProp)
            .SendAsync()
            .ThrowIfError()
            .DonotCapture();
        Assert.False(res.HasError);

        var token = res.ResponseString.ToJToken();

        var gzip = token["gzip"];
        Assert.NotNull(gzip);
        Assert.Equal(setProp, gzip.ToObject<bool>());

        var body = token["body"];
        Assert.NotNull(body);
        var actual = body.ToObject<Dictionary<string, string>>();
        Assert.Equal(expected, actual);
    }
}