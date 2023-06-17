namespace FclEx.Utils;

public static class PollyHelper
{
    public static readonly SleepDurationProvider DefaultSleepDurationProvider = retryAttempt => TimeSpan.FromSeconds(1 + retryAttempt);

    public static IAsyncPolicy<HttpResponseMessage> GetHttpRetryPolicy(int retryCount = 2, SleepDurationProvider? sleepDurationProvider = null)
    {
        sleepDurationProvider ??= DefaultSleepDurationProvider;
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>() // thrown by Polly's TimeoutPolicy if the inner call times out
            .WaitAndRetryAsync(retryCount, sleepDurationProvider);
    }

    public static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(TimeSpan? timeout = null)
    {
        return Policy.TimeoutAsync<HttpResponseMessage>(timeout ?? TimeSpan.FromMinutes(1));
    }

    public static IAsyncPolicy<HttpResponseMessage> GetConnectTimeoutPolicy(int retryCount = 2, SleepDurationProvider? sleepDurationProvider = null)
    {
        sleepDurationProvider ??= DefaultSleepDurationProvider;
        return Policy<HttpResponseMessage>
            .Handle<Exception>(m => m.EnumerateInner().Any(m => m.Message.Contains("within the configured ConnectTimeout")))
            .WaitAndRetryAsync(retryCount, sleepDurationProvider);
    }
}