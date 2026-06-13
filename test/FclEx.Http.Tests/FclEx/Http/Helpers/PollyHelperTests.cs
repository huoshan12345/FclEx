namespace FclEx.Http.Helpers;

public class PollyHelperTests
{
    [Fact]
    public void DefaultSleepDurationProvider_AddsOneSecondToRetryAttempt()
    {
        Assert.Equal(TimeSpan.FromSeconds(2), PollyHelper.DefaultSleepDurationProvider(1));
        Assert.Equal(TimeSpan.FromSeconds(4), PollyHelper.DefaultSleepDurationProvider(3));
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.RequestTimeout)]
#if NET5_0_OR_GREATER
    [InlineData(HttpStatusCode.TooManyRequests)]
#else
    [InlineData((HttpStatusCode)429)]
#endif
    public async Task GetHttpRetryPolicy_WhenStatusCodeIsRetryable_RetriesConfiguredNumberOfTimes(HttpStatusCode statusCode)
    {
        var attempts = 0;
        var policy = PollyHelper.GetHttpRetryPolicy(2, _ => TimeSpan.Zero);

        using var response = await policy.ExecuteAsync(() =>
        {
            attempts++;
            return Task.FromResult(new HttpResponseMessage(attempts <= 2 ? statusCode : HttpStatusCode.OK));
        });

        Assert.Equal(3, attempts);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetHttpRetryPolicy_WhenStatusCodeIsNotRetryable_DoesNotRetry()
    {
        var attempts = 0;
        var policy = PollyHelper.GetHttpRetryPolicy(2, _ => TimeSpan.Zero);

        using var response = await policy.ExecuteAsync(() =>
        {
            attempts++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest));
        });

        Assert.Equal(1, attempts);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetIORetryPolicy_WhenInnerExceptionIsIOException_RetriesConfiguredNumberOfTimes()
    {
        var attempts = 0;
        var policy = PollyHelper.GetIORetryPolicy(2, _ => TimeSpan.Zero);

        using var response = await policy.ExecuteAsync(() =>
        {
            attempts++;
            return attempts <= 2
                ? Task.FromException<HttpResponseMessage>(new InvalidOperationException("outer", new IOException("io")))
                : Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });

        Assert.Equal(3, attempts);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetIORetryPolicy_WhenExceptionTreeHasNoIOException_DoesNotRetry()
    {
        var attempts = 0;
        var policy = PollyHelper.GetIORetryPolicy(2, _ => TimeSpan.Zero);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => policy.ExecuteAsync(() =>
        {
            attempts++;
            return Task.FromException<HttpResponseMessage>(new InvalidOperationException("outer", new TimeoutException("timeout")));
        }));

        Assert.Equal("outer", ex.Message);
        Assert.Equal(1, attempts);
    }

    [Fact]
    public async Task GetConnectTimeoutPolicy_WhenNestedExceptionMessageMatches_RetriesConfiguredNumberOfTimes()
    {
        var attempts = 0;
        var policy = PollyHelper.GetConnectTimeoutPolicy(2, _ => TimeSpan.Zero);

        using var response = await policy.ExecuteAsync(() =>
        {
            attempts++;
            return attempts <= 2
                ? Task.FromException<HttpResponseMessage>(
                    new InvalidOperationException("outer", new TimeoutException("The operation timed out within the configured ConnectTimeout.")))
                : Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });

        Assert.Equal(3, attempts);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetConnectTimeoutPolicy_WhenExceptionMessageDoesNotMatch_DoesNotRetry()
    {
        var attempts = 0;
        var policy = PollyHelper.GetConnectTimeoutPolicy(2, _ => TimeSpan.Zero);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => policy.ExecuteAsync(() =>
        {
            attempts++;
            return Task.FromException<HttpResponseMessage>(new InvalidOperationException("different timeout"));
        }));

        Assert.Equal("different timeout", ex.Message);
        Assert.Equal(1, attempts);
    }

    [Fact]
    public async Task GetTimeoutPolicy_WhenOperationExceedsTimeout_ThrowsTimeoutRejectedException()
    {
        var policy = PollyHelper.GetTimeoutPolicy(TimeSpan.FromMilliseconds(20));

        await Assert.ThrowsAsync<TimeoutRejectedException>(() => policy.ExecuteAsync(async token =>
        {
            await Task.Delay(TimeSpan.FromSeconds(5), token);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }, CancellationToken.None));
    }

    [RetryTheory]
    [InlineData(1, 0.1)]
    [InlineData(2, 0.1)]
    [InlineData(3, 0.1)]
    public async Task GetConnectTimeoutPolicy_WhenConnectTimeoutOccurs_RetriesAndPreservesExpectedElapsedTime(int retryCount, double timeoutSeconds)
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

#if NET5_0_OR_GREATER
        Assert.Contains(ex.EnumerateInner(), m => m.Message.Contains("configured ConnectTimeout"));        
#endif
        var executeTime = timeout.Multiply(retryCount + 1);
        Assert.Equal(executeTime, time, TimeSpan.FromSeconds(0.4));
    }
}
