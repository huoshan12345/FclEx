namespace FclEx.Actions;

public class HttpRequestActionTests
{
    [Fact]
    public async Task MutipleActions_Tests()
    {
        var uri = UrlUtil.Combine(GlobalConstants.TestUrl, "/api/post");
        using var http = HttpClientService.Default;
        var (successful, data, ex, _) = await HttpRequest.Post(uri)
            .JsonContent(Enumerable.Range(1, 10).ToList())
            .ToAction(http)
            .ReadJson<List<int>>("body")
            .NextRequest(m => HttpRequest.Post(uri).JsonContent(m.Select(x => x.ToString()).ToDictionary(x => x, x => x + x)), http)
            .ReadJson<Dictionary<string, string>>("body")
            .ExecuteAsync()
            .DonotCapture();

        Assert.True(successful, ex?.Message);

        var dic = Enumerable.Range(1, 10)
            .Select(x => x.ToString())
            .ToDictionary(x => x, x => x + x);
        Assert.Equal(dic, data);
    }
}