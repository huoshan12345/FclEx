namespace FclEx.Http.Actions;

public class HttpRequestActionTests : HttpServerTests
{
    [Fact]
    public async Task MultipleActions_Tests()
    {
        if (HasApiServer == false)
            return;

        var path = new Uri("api/post", UriKind.RelativeOrAbsolute);
        var (successful, data, ex, _) = await HttpRequest.Post(path)
            .JsonContent(Enumerable.Range(1, 10).ToList())
            .ToAction(TestHttp)
            .ReadJson<List<int>>()
            .NextRequest(m => HttpRequest.Post(path).JsonContent(m.Select(x => x.ToString()).ToDictionary(x => x, x => x + x)), TestHttp)
            .ReadJson<Dictionary<string, string>>()
            .ExecuteAsync();

        Assert.True(successful, ex?.Message);

        var dic = Enumerable.Range(1, 10)
            .Select(x => x.ToString())
            .ToDictionary(x => x, x => x + x);
        Assert.Equal(dic, data);
    }
}