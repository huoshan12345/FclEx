namespace FclEx.Http;

public class HttpRequestExtensionsTests
{
    private static async ValueTask SuccessRequest()
    {
        await HttpRequest.Get("https://www.baidu.com")
            .SendAsync()
            .ThrowIfError();
        await Task.Delay(TimeSpan.FromSeconds(3));
    }

    private static async Task SuccessRequestWrap()
    {
        await HttpRequest.Get("https://www.baidu.com")
            .SendAsync()
            .ThrowIfError();
        await Task.Delay(TimeSpan.FromSeconds(3));
    }

    private static async Task TimeoutRequestWrap()
    {
        await HttpRequest.Get("https://www.google.com")
            .TotalTimeout(TimeSpan.FromSeconds(5))
            .SendAsync()
            .ThrowIfError();
    }

    [Fact]
    public async Task ThrowIfError_ValueTask_Test()
    {
        await Assert.ThrowsAnyAsync<Exception>(async () =>
            await HttpRequest.Get("http://localhost:9999")
                .SendAsync()
                .ThrowIfError());
    }

    [Fact]
    public async Task ThrowIfError_ValueTask_Execute_Test()
    {
        var flag = false;
        var r = await Operate.ExecuteAsync(() => TimeoutRequestWrap())
            .Error(e =>
            {
                flag = true;
                Assert.IsType<TaskCanceledException>(e);
            });
        Assert.False(!flag ^ r.Success);
    }

    [Fact]
    public async Task Execute_Test()
    {
        var flag = false;
        var r = await Operate.ExecuteAsync(() => SuccessRequestWrap())
            .Error(e =>
            {
                flag = true;
                Assert.IsType<OperationCanceledException>(e);
            });
        Assert.False(!flag ^ r.Success);
    }
}