using FclEx.Xunit;

namespace FclEx.Http.Helpers;

public class PollyHelperTests
{
    [RetryTheory]
    [InlineData(1, 0.5)]
    [InlineData(2, 0.5)]
    [InlineData(3, 0.5)]
    public async Task GetConnectTimeoutPolicy_Test(int retryCount, double timeoutSeconds)
    {
        if (TestHelper.IsGithubAction && retryCount > 1)
            return;

        var timeout = TimeSpan.FromSeconds(timeoutSeconds);
        var services = new ServiceCollection();

        services.AddHttpClient(string.Empty)
            .ConfigurePrimaryHttpMessageHandler(() => HttpClientHelper.CreateSocketsHttpHandler(new() { ConnectTimeout = timeout }))
            .AddPolicyHandler(PollyHelper.GetConnectTimeoutPolicy(retryCount, m => TimeSpan.Zero));

        var provider = services.BuildServiceProvider();

        var httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient();

        var watch = ValueStopwatch.StartNew();
        var ex = await Assert.ThrowsAnyAsync<TaskCanceledException>(() => httpClient.GetAsync("https://baidu.com:444/", HttpCompletionOption.ResponseHeadersRead));
        var time = watch.GetElapsedTime();

        Assert.Contains(ex.EnumerateInner(), m => m.Message.Contains("configured ConnectTimeout"));

        var executeTime = timeout.Multiply(retryCount + 1);
        Assert.Equal(executeTime, time, TimeSpan.FromSeconds(0.4));
    }
}