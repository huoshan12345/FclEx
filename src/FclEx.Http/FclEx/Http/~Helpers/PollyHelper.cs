namespace FclEx.Http;

public static class PollyHelper
{
    public static readonly SleepDurationProvider DefaultSleepDurationProvider = retryAttempt => TimeSpan.FromSeconds(1 + retryAttempt);

    public static IAsyncPolicy<HttpResponseMessage> GetHttpRetryPolicy(int retryCount = 2, SleepDurationProvider? sleepDurationProvider = null)
    {
        sleepDurationProvider ??= DefaultSleepDurationProvider;
        return Policy<HttpResponseMessage>
            .HandleResult(Filter)
            .Or<TimeoutRejectedException>() // thrown by Polly's TimeoutPolicy if the inner call times out
            .WaitAndRetryAsync(retryCount, sleepDurationProvider);

        static bool Filter(HttpResponseMessage m)
        {
            return m.StatusCode.IsServerError()
#if NETSTANDARD2_0
                   || m.StatusCode.ToInt() == 429
#else
                   || m.StatusCode == HttpStatusCode.TooManyRequests
#endif
                   || m.StatusCode == HttpStatusCode.RequestTimeout;
        }
    }

    public static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(TimeSpan? timeout = null)
    {
        return Policy.TimeoutAsync<HttpResponseMessage>(timeout ?? TimeSpan.FromMinutes(1));
    }

    public static IAsyncPolicy<HttpResponseMessage> GetConnectTimeoutPolicy(int retryCount = 2, SleepDurationProvider? sleepDurationProvider = null)
    {
        sleepDurationProvider ??= DefaultSleepDurationProvider;
        return Policy<HttpResponseMessage>
            .Handle<Exception>(m => m.EnumerateInner().Any(x => x.Message.Contains("within the configured ConnectTimeout")))
            .WaitAndRetryAsync(retryCount, sleepDurationProvider);
    }

    // ReSharper disable once InconsistentNaming
    public static IAsyncPolicy<HttpResponseMessage> GetIORetryPolicy(int retryCount = 2, SleepDurationProvider? sleepDurationProvider = null)
    {
        sleepDurationProvider ??= DefaultSleepDurationProvider;
        return Policy<HttpResponseMessage>
            .Handle<Exception>(m => m.EnumerateInner().Any(x => x is IOException))
            .WaitAndRetryAsync(retryCount, sleepDurationProvider);
    }
}