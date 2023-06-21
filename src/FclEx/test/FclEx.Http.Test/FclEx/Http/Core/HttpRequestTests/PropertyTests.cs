using System.Net.Http.Headers;
using System.Reflection;
using Newtonsoft.Json;

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
        var req = HttpRequest.Get(CharSetTestCase.Url);
        if (value)
            req.CharSet(CharSetTestCase.CharSet);

        var res = await http.SendAsync(req)
            .ThrowIfError()
            .DonotCapture();

        Assert.Equal(value, res.ResponseString.Contains(CharSetTestCase.Keyword));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task FallbackCharSet_Test(bool value)
    {
        using var http = new HttpClientService();
        var req = HttpRequest.Get(CharSetTestCase.Url);
        if (value)
            req.CharSet(CharSetTestCase.CharSet);

        var res = await http.SendAsync(req)
            .ThrowIfError()
            .DonotCapture();
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

        var res = await http.SendAsync(req)
            .ThrowIfError()
            .DonotCapture();
        Assert.Equal(value, res.ResponseString.Contains(CharSetTestCase.Keyword));
    }

    private static readonly HttpClientService Http = HttpClientService.Create("http://127.0.0.1:8888", false);

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GZip_Test(bool value)
    {
        var random = new Random(1024);
        var expected = Enumerable.Range(1, 100).ToDictionary(m => m.ToString(), m => random.NextString(5));
        var res = await HttpRequest.Post(new Uri(GlobalConstants.TestUri, "api/gzip"))
            .AddData(expected!)
            .ConnectTimeout(TimeSpan.FromSeconds(30))
            .UseGZip(value)
            .SendAsync(Http)
            .ThrowIfError()
            .DonotCapture();

        Assert.False(res.HasError);

        var token = res.ResponseString.ToJToken();

        var headers = token["headers"]?.ToString();
        Assert.NotNull(headers);

        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        JsonConvert.PopulateObject(headers, result);

        var length = result.Get(HttpKnownHeaderNames.ContentLength, m => int.Parse(m));
        Assert.Equal(value ? 666 : 891, length);

        var gzip = token["gzip"];
        Assert.NotNull(gzip);
        Assert.Equal(value, gzip.ToObject<bool>());

        var body = token["body"];
        Assert.NotNull(body);
        var actual = body.ToObject<Dictionary<string, string>>();
        Assert.Equal(expected, actual);
    }
}