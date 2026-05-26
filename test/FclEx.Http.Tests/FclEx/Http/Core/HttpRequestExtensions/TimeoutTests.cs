namespace FclEx.Http.Core.HttpRequestExtensions;

public class TimeoutTests : HttpServerTests
{
    [RetryTheory]
    [InlineData(0.1)]
    [InlineData(0.3)]
    public async Task ReadHeadersTimeout_Test(double timeoutSeconds)
    {
        if (HasApiServer == false)
            return;

        var http = HttpClientService.Create(m =>
        {
            m.BaseAddress = TestUri;
            m.RetryCount = 0;
        });
        var timeout = TimeSpan.FromSeconds(timeoutSeconds);
        var task = HttpRequest.Get(TestApiPaths.Sleep)
            .AddQueryParam("seconds", "3")
            .ReadHeadersTimeout(timeout)
            .SendAsync(http)
            .ThrowIfError();

        var (successful, _, exception, elapsed) = await Operation.ExecuteAsync(() => task);
        Assert.False(successful);
        Assert.IsType<TaskCanceledException>(exception);
        Assert.Equal(timeout, elapsed, TimeSpan.FromSeconds(1));
    }
}