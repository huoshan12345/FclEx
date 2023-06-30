namespace FclEx.Http;

public static class PollyHelper
{
    public static readonly SleepDurationProvider DefaultSleepDurationProvider = retryAttempt => TimeSpan.FromSeconds(1 + retryAttempt);

    public static IAsyncPolicy<HttpResponseMessage> GetHttpRetryPolicy(int retryCount = 2, SleepDurationProvider? sleepDurationProvider = null)
    {
        sleepDurationProvider ??= DefaultSleepDurationProvider;
        return Policy<HttpResponseMessage>
            .HandleResult(m => m.StatusCode.IsServerError() || m.StatusCode == HttpStatusCode.TooManyRequests || m.StatusCode == HttpStatusCode.RequestTimeout)
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

    private static readonly string[] _canceledErrors =
    {
        "The operation was canceled.",
        "A task was canceled.",
    };

    // ReSharper disable once InconsistentNaming
    public static IAsyncPolicy<HttpResponseMessage> GetIORetryPolicy(int retryCount = 2, SleepDurationProvider? sleepDurationProvider = null)
    {
        sleepDurationProvider ??= DefaultSleepDurationProvider;
        return Policy<HttpResponseMessage>
            .Handle<Exception>(m => m.EnumerateInner().Any(m => m is IOException))
            .WaitAndRetryAsync(retryCount, sleepDurationProvider);
    }

    public static IAsyncPolicy<HttpResponseMessage> GetCancelPolicy(int retryCount = 2, SleepDurationProvider? sleepDurationProvider = null)
    {
        sleepDurationProvider ??= DefaultSleepDurationProvider;
        return Policy<HttpResponseMessage>
            .Handle<TaskCanceledException>(m => m.InnerException is null && _canceledErrors.Contains(m.Message))
            .WaitAndRetryAsync(retryCount, sleepDurationProvider);
    }
}