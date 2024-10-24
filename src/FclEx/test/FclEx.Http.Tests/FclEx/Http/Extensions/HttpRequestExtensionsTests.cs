namespace FclEx.Http;

public class HttpRequestExtensionsTests
{
    private static async Task SuccessRequestWrap()
    {
        await HttpRequest.Get("https://www.baidu.com")
            .SendAsync()
            .ThrowIfError();
    }

    private static async Task TimeoutRequestWrap()
    {
        var http = HttpClientService.Create(m => m.RetryCount = 0);
        await HttpRequest.Get("https://www.google.com")
            .TotalTimeout(TimeSpan.FromSeconds(1))
            .SendAsync(http)
            .ThrowIfError();
    }

    [Fact]
    public async Task ThrowIfError_ValueTask_Test()
    {
        var http = HttpClientService.Create(m =>
        {
            m.ConnectTimeout = TimeSpan.FromMilliseconds(200);
            m.RetryCount = 1;
            m.SleepDurationProvider = m => TimeSpan.FromMilliseconds(100 * m);
        });
        await Assert.ThrowsAnyAsync<Exception>(async () =>
            await HttpRequest.Get("http://localhost:9999")
                .TotalTimeout(TimeSpan.FromMilliseconds(200))
                .SendAsync(http)
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