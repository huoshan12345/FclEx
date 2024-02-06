namespace FclEx.Http.Core.HttpRequestTests;

public class TimeoutTests
{
    [RetryTheory]
    [InlineData(1)]
    [InlineData(3)]
    public async Task ConnectTimeout_Test(int timeoutSeconds)
    {
        var http = HttpClientService.Create(m => m.RetryCount = 0);
        var timeout = TimeSpan.FromSeconds(timeoutSeconds);
        var task = HttpRequest.Get("https://httpstat.us/504?sleep=60000")
            .ReadHeadersTimeout(timeout)
            .SendAsync(http)
            .ThrowIfError();

        var (successful, _, exception, elapsed) = await Operate.ExecuteAsync(() => task);
        Assert.False(successful);
        Assert.IsType<TaskCanceledException>(exception);
        AssertExt.Equal(timeout, elapsed, TimeSpan.FromSeconds(1));
    }
}