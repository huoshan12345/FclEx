namespace FclEx.Http.Core;

public class HttpRequestTests
{
    private static async Task SuccessRequestWrap()
    {
        var url = TestUrls.First();
        await HttpRequest.Get(url)
            .SendAsync()
            .ThrowIfError();
    }

    private static async Task TimeoutRequestWrap()
    {
        var http = HttpClientService.Create(m => m.RetryPolicyOptions.RetryCount = 0);
        await HttpRequest.Get("https://www.google.com")
            .TotalTimeout(TimeSpan.FromSeconds(0.1))
            .SendAsync(http)
            .ThrowIfError();
    }

    [Fact]
    public async Task ThrowIfError_ValueTask_Test()
    {
        var http = HttpClientService.Create(m =>
        {
            m.HandlerOptions.ConnectTimeout = TimeSpan.FromMilliseconds(200);
            m.RetryPolicyOptions.RetryCount = 1;
            m.RetryPolicyOptions.SleepDurationProvider = x => TimeSpan.FromMilliseconds(100 * x);
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
        var r = await Operation.ExecuteAsync(TimeoutRequestWrap)
            .OnException(e =>
            {
                flag = true;
                Assert.IsType<TaskCanceledException>(e);
            });
        Assert.False(!flag ^ r.IsSuccess);
    }

    [Fact]
    public async Task Execute_Test()
    {
        var flag = false;
        var r = await Operation.ExecuteAsync(SuccessRequestWrap)
            .OnException(e =>
            {
                flag = true;
                Assert.IsType<OperationCanceledException>(e);
            });
        Assert.False(!flag ^ r.IsSuccess);
    }
}