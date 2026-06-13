namespace FclEx.Http.Actions;

public class HttpRequestActionTests : HttpServerTests
{
    [Fact]
    public async Task ExecuteAsync_WhenActionsAreChained_PassesPreviousResultToNextRequest()
    {
        if (HasApiServer == false)
            return;

        var path = new Uri(TestApiPaths.Post, UriKind.RelativeOrAbsolute);
        var (successful, data, ex, _) = await HttpRequest.Post(path)
            .JsonContent(Enumerable.Range(1, 10).ToList())
            .ToAction(TestHttp)
            .ReadJsonAs<List<int>>()
            .ThenRequest(m => HttpRequest.Post(path).JsonContent(m.Select(x => x.ToString()).ToDictionary(x => x, x => x + x)), TestHttp)
            .ReadJsonAs<Dictionary<string, string>>()
            .ExecuteAsync();

        Assert.True(successful, ex?.Message);

        var dic = Enumerable.Range(1, 10)
            .Select(x => x.ToString())
            .ToDictionary(x => x, x => x + x);
        Assert.Equal(dic, data);
    }
}
