namespace FclEx.Http;

public class HttpClientOptions : SocketsHttpHandlerOptions, IEqualityComparer<HttpClientOptions>
{
    public static readonly SleepDurationProvider DefaultSleepDurationProvider = retryAttempt => TimeSpan.FromSeconds(1 + retryAttempt);

    public TimeSpan ExecutionTimeout { get; set; } = TimeSpan.FromMinutes(1);
    public int RetryCount { get; set; } = 2;

    /// <summary>
    /// Indicates whether or not update <see cref="TotalTimeout"/> when it is less than a total timeout that is computed with <see cref="ExecutionTimeout"/> and <see cref="RetryCount"/>
    /// </summary>
    public bool AutoUpdateTotalTimeout { get; set; } = true;
    public SleepDurationProvider SleepDurationProvider { get; set; } = DefaultSleepDurationProvider;

    /// <summary>
    /// Will be used as <see cref="HttpClient.Timeout"/>
    /// </summary>
    public TimeSpan TotalTimeout { get; set; } = TimeSpan.FromMinutes(2);

    public new static readonly HttpClientOptions Default = new();

    public bool Equals(HttpClientOptions? x, HttpClientOptions? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;


        return base.Equals(x, y)
               && x.ExecutionTimeout.Equals(y.ExecutionTimeout)
               && x.RetryCount == y.RetryCount
               && x.AutoUpdateTotalTimeout == y.AutoUpdateTotalTimeout
               && x.SleepDurationProvider.Equals(y.SleepDurationProvider)
               && x.TotalTimeout.Equals(y.TotalTimeout);
    }

    public int GetHashCode(HttpClientOptions obj)
    {
        return HashCode.Combine(
            base.GetHashCode(obj),
            obj.ExecutionTimeout,
            obj.RetryCount,
            obj.AutoUpdateTotalTimeout,
            obj.SleepDurationProvider,
            obj.TotalTimeout);
    }
}