namespace FclEx.Http.Core.HttpReqTests;

public class TimeoutTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    public async Task ConnectTimeout_Test(int timeoutSeconds)
    {
        var timeout = TimeSpan.FromSeconds(timeoutSeconds);
        var req = HttpReq.Get("https://httpstat.us/504?sleep=60000")
            .ConnectTimeout(timeout);

        var (successful, _, exception, elapsed) = await Operate.ExecuteAsync(async () => await req.SendAsync().ThrowIfError());
        Assert.False(successful);
        Assert.IsType<TaskCanceledException>(exception);
        AssertExt.Equal(timeout, elapsed, TimeSpan.FromSeconds(1));
    }
}