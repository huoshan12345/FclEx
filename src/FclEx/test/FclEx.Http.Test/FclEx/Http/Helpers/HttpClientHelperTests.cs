using Polly.Timeout;

namespace FclEx.Http.Helpers;

public class HttpClientHelperTests
{
    private readonly ITestOutputHelper _output;

    public HttpClientHelperTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task GetRetryPolicy_Timeout_Test()
    {
        var timeout = TimeSpan.FromSeconds(1);
        const int retryCount = 2;
        var services = new ServiceCollection();

        services.AddHttpClient(Options.DefaultName)
            .ConfigurePrimaryHttpMessageHandler(() => HttpClientHelper.CreateSocketsHttpHandler(new() { ConnectTimeout = TimeSpan.FromHours(1) })) // NOTE: to test HttpClient.Timeout, we need to make it less than SocketsHttpHandler.ConnectTimeout
            .AddRetryPolicy(timeout, 2, true, m => TimeSpan.Zero);

        var provider = services.BuildServiceProvider();

        var httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient();

        var watch = ValueStopwatch.StartNew();
        await Assert.ThrowsAnyAsync<TimeoutRejectedException>(() => httpClient.GetAsync("https://www.google.com:444/", HttpCompletionOption.ResponseHeadersRead));
        var time = watch.GetElapsedTime();

        var executeTime = timeout.Multiply(retryCount + 1);
        AssertExt.Equal(executeTime, time, TimeSpan.FromSeconds(0.9));
    }

    [Fact]
    public async Task HttpClient_Timeout_Test()
    {
        var handler = HttpClientHelper.CreateSocketsHttpHandler(new() { ConnectTimeout = TimeSpan.FromHours(1) });
        using var httpClient = new HttpClient(handler, true) { Timeout = TimeSpan.FromSeconds(1) };

        var watch = ValueStopwatch.StartNew();
        var ex = await Assert.ThrowsAnyAsync<TaskCanceledException>(() => httpClient.GetAsync("https://www.google.com:444/", HttpCompletionOption.ResponseHeadersRead));
        var time = watch.GetElapsedTime();

        Assert.Contains("configured HttpClient.Timeout", ex.Message);
        Assert.NotNull(ex.InnerException);

        AssertExt.Equal(TimeSpan.FromSeconds(1), time, TimeSpan.FromSeconds(0.5));
    }
}