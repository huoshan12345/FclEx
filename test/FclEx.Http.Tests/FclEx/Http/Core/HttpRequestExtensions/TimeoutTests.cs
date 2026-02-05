namespace FclEx.Http.Core.HttpRequestExtensions;

public class TimeoutTests
{
    [RetryTheory(Skip = "test url is not avaible")]
    [InlineData(0.1)]
    [InlineData(0.3)]
    public async Task ConnectTimeout_Test(double timeoutSeconds)
    {
        var http = HttpClientService.Create(m => m.RetryCount = 0);
        var timeout = TimeSpan.FromSeconds(timeoutSeconds);
        var task = HttpRequest.Get("https://httpstat.us/504?sleep=60000")
            .ReadHeadersTimeout(timeout)
            .SendAsync(http)
            .ThrowIfError();

        var (successful, _, exception, elapsed) = await Operation.ExecuteAsync(() => task);
        Assert.False(successful);
        Assert.IsType<TaskCanceledException>(exception);
        Assert.Equal(timeout, elapsed, TimeSpan.FromSeconds(1));
    }
}