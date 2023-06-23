namespace FclEx.Http.Core;

public class HttpResponseExtensionsTests
{
    [Fact]
    public async Task Task_HttpResponse_ThrowIfError_Test()
    {
        var error = nameof(Task_HttpResponse_ThrowIfError_Test);
        var task = Task.Run(async () =>
        {
            await Task.Yield();
            return HttpResponse.CreateError(HttpRequest.Get("http://localhost"), new SimpleException(error));
        }).ThrowIfError();

        var ex = await Assert.ThrowsAsync<SimpleException>(() => task);
        Assert.Equal(error, ex.Message);
    }

    [Fact]
    public async Task Task_HttpResponse_ReadJsonAs_Test()
    {
        var random = new Random(1024);
        var expected = Enumerable.Range(1, 3).ToDictionary(m => m.ToString(), m => random.NextString(5));
        var actual = await HttpRequest.Post("api/post")
            .AddData(expected!)
            .ReadHeadersTimeout(TimeSpan.FromSeconds(5))
            .SendAsync(TestHttp)
            .ReadJsonAs<Dictionary<string, string>>("body")
            .DonotCapture();

        Assert.True(actual.Success, actual.Exception?.ToString());
        Assert.Equal(expected, actual.Value);
    }


    [Fact]
    public async Task Task_HttpResponse_ReadJsonAsRequired_Test()
    {
        var random = new Random(1024);
        var expected = Enumerable.Range(1, 3).ToDictionary(m => m.ToString(), m => random.NextString(5));
        var actual = await HttpRequest.Post("api/post")
            .AddData(expected!)
            .ReadHeadersTimeout(TimeSpan.FromSeconds(5))
            .SendAsync(TestHttp)
            .ReadJsonAsRequired<Dictionary<string, string>>("body")
            .DonotCapture();

        Assert.Equal(expected, actual);
    }
}