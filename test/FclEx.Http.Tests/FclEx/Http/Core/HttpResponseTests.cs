namespace FclEx.Http.Core;

public class HttpResponseTests : HttpServerTests
{
    [Fact]
    public void HttpResponse_ReadJsonAs_WhenPathMatches_ReturnsDeserializedValue()
    {
        var response = CreateResponse("""{"data":{"count":3}}""");

        var actual = response.ReadJsonAs<int>("data.count");

        Assert.True(actual.IsSuccess, actual.Exception?.ToString());
        Assert.Equal(3, actual.Value);
    }

    [Fact]
    public void HttpResponse_ReadJsonAs_WhenPathDoesNotMatch_ReturnsError()
    {
        var response = CreateResponse("""{"data":{"count":3}}""");

        var actual = response.ReadJsonAs<int>("missing.count");

        Assert.True(actual.IsError);
        Assert.Contains("missing.count", actual.Exception!.Message);
    }

    [Fact]
    public async Task Task_HttpResponse_ThrowIfError_Test()
    {
        const string error = nameof(Task_HttpResponse_ThrowIfError_Test);
        var task = Task.Run(async () =>
        {
            await Task.Yield();
            return HttpResponse.FromError(HttpRequest.Get("http://localhost"), new SimpleException(error));
        }).ThrowIfError();

        var ex = await Assert.ThrowsAsync<SimpleException>(() => task);
        Assert.Equal(error, ex.Message);
    }

    [Fact]
    public async Task Task_HttpResponse_ReadJsonAs_Test()
    {
        if (HasApiServer == false)
            return;

        var random = new Random(1024);
        var expected = Enumerable.Range(1, 3).ToDictionary(m => m.ToString(), m => random.NextString(5));
        var actual = await HttpRequest.Post("api/post")
            .JsonContent(expected)
            .ReadHeadersTimeout(TimeSpan.FromSeconds(5))
            .SendAsync(TestHttp)
            .ReadJsonAs<Dictionary<string, string>>();

        Assert.True(actual.IsSuccess, actual.Exception?.ToString());
        Assert.Equal(expected, actual.Value);
    }


    [Fact]
    public async Task Task_HttpResponse_ReadJsonAsRequired_Test()
    {
        if (HasApiServer == false)
            return;

        var random = new Random(1024);
        var expected = Enumerable.Range(1, 3).ToDictionary(m => m.ToString(), m => random.NextString(5));
        var actual = await HttpRequest.Post("api/post")
            .JsonContent(expected)
            .ReadHeadersTimeout(TimeSpan.FromSeconds(5))
            .SendAsync(TestHttp)
            .ReadJsonAsRequired<Dictionary<string, string>>();

        Assert.Equal(expected, actual);
    }

    private static HttpResponse CreateResponse(string responseString)
    {
        var response = new HttpResponse(HttpRequest.Get("https://example.com/api"));
        typeof(HttpResponse)
            .GetProperty(nameof(HttpResponse.ResponseString))!
            .SetValue(response, responseString);
        return response;
    }
}
