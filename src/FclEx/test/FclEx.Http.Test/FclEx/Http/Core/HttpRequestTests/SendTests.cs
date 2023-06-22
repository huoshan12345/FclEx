namespace FclEx.Http.Core.HttpRequestTests;

public class SendTests
{
    public static IList<string> Urls => new[]
    {
        "https://www.baidu.com/",
        "https://www.qq.com/",
        "https://www.google.com.hk/"
    };

    public static IEnumerable<object[]> Cases => Urls
        .Select(m => new object[] { m });

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task Get_Test(string url)
    {
        var res = await HttpRequest.Get(url)
            .ReadHeadersTimeout(TimeSpan.FromSeconds(5))
            .SendAsync()
            .DonotCapture();
        res.ThrowIfError();
    }

    [Fact]
    public async Task Form_Test()
    {
        var random = new Random(1024);
        var expected = Enumerable.Range(1, 3).ToDictionary(m => m.ToString(), m => random.NextString(5));
        var res = await HttpRequest.Post("api/post")
            .AddData(expected!)
            .ReadHeadersTimeout(TimeSpan.FromSeconds(5))
            .SendAsync(TestHttp)
            .ThrowIfError()
            .DonotCapture();
        Assert.False(res.HasError);
        var body = res.ResponseString.ToJToken()["body"];
        Assert.NotNull(body);
        var actual = body.ToObject<Dictionary<string, string>>();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task Json_Test()
    {
        var list = Enumerable.Range(1, 10).ToList();
        var res = await HttpRequest.Post("api/post")
            .JsonContent(list)
            .SendAsync(TestHttp)
            .ThrowIfError()
            .DonotCapture();
        Assert.False(res.HasError);
        var body = res.ResponseString.ToJToken()["body"];
        Assert.NotNull(body);
        var actual = body.ToObject<List<int>>();
        Assert.True(list.SequenceEqual(actual!));
    }
}