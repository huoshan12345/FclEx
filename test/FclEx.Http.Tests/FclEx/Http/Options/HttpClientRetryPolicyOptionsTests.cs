namespace FclEx.Http.Options;

public class HttpClientRetryPolicyOptionsTests
{
    [Fact]
    public void Constructor_UsesExpectedDefaults()
    {
        var options = new HttpClientRetryPolicyOptions();

        Assert.Equal(TimeSpan.FromMinutes(1), options.ExecutionTimeout);
        Assert.Equal(2, options.RetryCount);
        Assert.True(options.AutoUpdateTotalTimeout);
        Assert.Equal(TimeSpan.Zero, options.SleepDurationProvider(10));
        Assert.Same(HttpClientRetryPolicyOptions.DefaultSleepDurationProvider, options.SleepDurationProvider);
    }
}
