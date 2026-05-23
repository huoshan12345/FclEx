namespace FclEx.Http;

public record HttpClientOptions : SocketsHttpHandlerOptions // use record so that with expression can be used
{
    public static readonly SleepDurationProvider DefaultSleepDurationProvider = retryAttempt => TimeSpan.Zero;

    public Uri? BaseAddress { get; set; }
#if NET6_0_OR_GREATER
    public HttpVersionPolicy HttpVersionPolicy { get; set; } = HttpVersionPolicy.RequestVersionOrLower;
    public Version HttpVersion { get; set; } = System.Net.HttpVersion.Version11;
#endif
    public TimeSpan ExecutionTimeout { get; set; } = TimeSpan.FromMinutes(1);
    public int RetryCount { get; set; } = 2;

    /// <summary>
    /// Indicates whether update <see cref="TotalTimeout"/> when it is less than a total timeout that is computed with <see cref="ExecutionTimeout"/> and <see cref="RetryCount"/>
    /// </summary>
    public bool AutoUpdateTotalTimeout { get; set; } = true;
    public SleepDurationProvider SleepDurationProvider { get; set; } = DefaultSleepDurationProvider;

    /// <summary>
    /// Will be used as <see cref="HttpClient.Timeout"/>
    /// </summary>
    public TimeSpan TotalTimeout { get; set; } = TimeSpan.FromMinutes(2);
}