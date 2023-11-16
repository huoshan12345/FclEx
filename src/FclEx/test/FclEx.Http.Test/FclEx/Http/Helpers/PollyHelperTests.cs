using Polly;

namespace FclEx.Http.Helpers;

public class PollyHelperTests
{
    private readonly ITestOutputHelper _output;

    public PollyHelperTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [RetryTheory]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
    [InlineData(2, 2)]
    public async Task GetConnectTimeoutPolicy_Test(int retryCount, int timeoutSeconds)
    {
        var timeout = TimeSpan.FromSeconds(timeoutSeconds);
        var services = new ServiceCollection();

        services.AddHttpClient(Options.DefaultName)
            .ConfigurePrimaryHttpMessageHandler(() => HttpClientHelper.CreateSocketsHttpHandler(new() { ConnectTimeout = timeout }))
            .AddPolicyHandler(PollyHelper.GetConnectTimeoutPolicy(retryCount, m => TimeSpan.Zero));

        var provider = services.BuildServiceProvider();

        var httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient();

        var watch = ValueStopwatch.StartNew();
        var ex = await Assert.ThrowsAnyAsync<TaskCanceledException>(() => httpClient.GetAsync("https://www.google.com:444/", HttpCompletionOption.ResponseHeadersRead));
        var time = watch.GetElapsedTime();

        ex.EnumerateInner().Should().Contain(m => m.Message.Contains("configured ConnectTimeout"));

        var executeTime = timeout.Multiply(retryCount + 1);
        AssertExt.Equal(executeTime, time, TimeSpan.FromSeconds(0.9));
    }
}