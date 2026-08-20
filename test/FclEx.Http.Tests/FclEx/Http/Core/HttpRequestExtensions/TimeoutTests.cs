namespace FclEx.Http.Core.HttpRequestExtensions;

public class TimeoutTests : HttpServerTests
{
    [RetryTheory]
    [InlineData(0.1)]
    [InlineData(0.3)]
    public async Task ReadHeadersTimeout_WhenServerDelaysHeaders_CancelsNearConfiguredTimeout(double timeoutSeconds)
    {
        Assert.SkipUnlessHasApiServer();

        var http = HttpClientService.Create(m =>
        {
            m.BaseAddress = TestUri;
            m.RetryPolicyOptions = new()
            {
                RetryCount = 0,
                SleepDurationProvider = _ => TimeSpan.Zero,
            };
        });
        var timeout = TimeSpan.FromSeconds(timeoutSeconds);
        var task = HttpRequest.Get(TestApiPaths.Sleep)
            .AddQueryParam("seconds", "3")
            .ReadHeadersTimeout(timeout)
            .SendAsync(http)
            .ThrowIfError();

        var (successful, value, exception, elapsed) = await Operation.ExecuteAsync(t => task);
        Assert.False(successful);
        Assert.IsType<TaskCanceledException>(exception);
        Assert.Equal(timeout, elapsed, TimeSpan.FromSeconds(1));
    }
}
