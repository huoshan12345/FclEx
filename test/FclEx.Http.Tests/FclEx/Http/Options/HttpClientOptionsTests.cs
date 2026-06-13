namespace FclEx.Http.Options;

public class HttpClientOptionsTests
{
    [Fact]
    public void Constructor_UsesExpectedDefaults()
    {
        var options = new HttpClientOptions();

        Assert.Null(options.BaseAddress);
        Assert.Equal(TimeSpan.FromMinutes(2), options.TotalTimeout);
        Assert.NotNull(options.HandlerOptions);
        Assert.NotNull(options.RetryPolicyOptions);
#if NET6_0_OR_GREATER
        Assert.Equal(HttpVersionPolicy.RequestVersionOrLower, options.HttpVersionPolicy);
        Assert.Equal(HttpVersion.Version11, options.HttpVersion);
#endif
    }
}
