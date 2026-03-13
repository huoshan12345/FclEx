namespace FclEx.Http.Helpers;

public class HttpClientHelperTests
{
    private readonly ITestOutputHelper _output;

    public HttpClientHelperTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [RetryFact]
    public async Task GetRetryPolicy_Timeout_Test()
    {
        var timeout = TimeSpan.FromSeconds(0.2);
        const int retryCount = 2;
        var services = new ServiceCollection();

        services.AddHttpClient(string.Empty)
            // NOTE: to test HttpClient.Timeout, we need to make it less than SocketsHttpHandler.ConnectTimeout
            .ConfigurePrimaryHttpMessageHandler(() => HttpClientHelper.CreateSocketsHttpHandler(new() { ConnectTimeout = TimeSpan.FromHours(1) }))
            .AddRetryPolicy(timeout, 2, true, m => TimeSpan.Zero);

        var provider = services.BuildServiceProvider();

        var httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient();

        var watch = ValueStopwatch.StartNew();
        await Assert.ThrowsAnyAsync<TimeoutRejectedException>(() => httpClient.GetAsync("https://google.com:444/", HttpCompletionOption.ResponseHeadersRead));
        var time = watch.GetElapsedTime();

        var executeTime = timeout.Multiply(retryCount + 1);
        Assert.Equal(executeTime, time, TimeSpan.FromSeconds(0.1));
    }

    [RetryFact]
    public async Task HttpClient_Timeout_Test()
    {
        var timeout = TimeSpan.FromSeconds(0.2);
        var handler = HttpClientHelper.CreateSocketsHttpHandler(new() { ConnectTimeout = TimeSpan.FromHours(1) });
        using var httpClient = new HttpClient(handler, true) { Timeout = timeout };

        var watch = ValueStopwatch.StartNew();
        var ex = await Assert.ThrowsAnyAsync<TaskCanceledException>(() => httpClient.GetAsync("https://google.com:444/", HttpCompletionOption.ResponseHeadersRead));
        var time = watch.GetElapsedTime();

        Assert.Contains("configured HttpClient.Timeout", ex.Message);
        Assert.NotNull(ex.InnerException);

        Assert.Equal(timeout, time, TimeSpan.FromSeconds(0.1));
    }
}