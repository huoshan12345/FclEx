namespace FclEx.Http;

/// <summary>
/// Factory methods for Polly policies used by FclEx HTTP clients.
/// </summary>
public static class PollyHelper
{
    /// <summary>
    /// Default retry delay provider used by helper-created policies.
    /// The first retry waits two seconds, then the delay increases by one second per retry attempt.
    /// </summary>
    public static readonly SleepDurationProvider DefaultSleepDurationProvider = retryAttempt => TimeSpan.FromSeconds(1 + retryAttempt);

    /// <summary>
    /// Creates a retry policy for transient HTTP responses and Polly timeout rejections.
    /// The policy retries server errors, request timeout, and Too Many Requests responses.
    /// </summary>
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
#if !NET5_0_OR_GREATER
                   || m.StatusCode.ToInt() == 429
#else
                   || m.StatusCode == HttpStatusCode.TooManyRequests
#endif
                   || m.StatusCode == HttpStatusCode.RequestTimeout;
        }
    }

    /// <summary>
    /// Creates a per-execution timeout policy for HTTP responses.
    /// A null timeout uses one minute.
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(TimeSpan? timeout = null)
    {
        return Policy.TimeoutAsync<HttpResponseMessage>(timeout ?? TimeSpan.FromMinutes(1));
    }

    /// <summary>
    /// Creates a retry policy for socket connect-timeout failures.
    /// The current detection matches inner exception messages emitted for SocketsHttpHandler ConnectTimeout failures.
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetConnectTimeoutPolicy(int retryCount = 2, SleepDurationProvider? sleepDurationProvider = null)
    {
        sleepDurationProvider ??= DefaultSleepDurationProvider;
        return Policy<HttpResponseMessage>
            .Handle<Exception>(m => m.EnumerateInner().Any(x => x.Message.Contains("within the configured ConnectTimeout")))
            .WaitAndRetryAsync(retryCount, sleepDurationProvider);
    }

    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// Creates a retry policy for exceptions whose inner exception chain contains an <see cref="IOException"/>.
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetIORetryPolicy(int retryCount = 2, SleepDurationProvider? sleepDurationProvider = null)
    {
        sleepDurationProvider ??= DefaultSleepDurationProvider;
        return Policy<HttpResponseMessage>
            .Handle<Exception>(m => m.EnumerateInner().Any(x => x is IOException))
            .WaitAndRetryAsync(retryCount, sleepDurationProvider);
    }
}
