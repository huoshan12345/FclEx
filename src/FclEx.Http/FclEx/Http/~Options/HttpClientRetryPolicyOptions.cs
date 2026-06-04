namespace FclEx.Http;

public record HttpClientRetryPolicyOptions
{
    public static readonly SleepDurationProvider DefaultSleepDurationProvider = retryAttempt => TimeSpan.Zero;

    public TimeSpan ExecutionTimeout { get; set; } = TimeSpan.FromMinutes(1);
    public int RetryCount { get; set; } = 2;

    /// <summary>
    /// Indicates whether update <see cref="HttpClient.Timeout"/> when it is less than a total timeout that is computed with <see cref="ExecutionTimeout"/> and <see cref="RetryCount"/>
    /// </summary>
    public bool AutoUpdateTotalTimeout { get; set; } = true;
    public SleepDurationProvider SleepDurationProvider { get; set; } = DefaultSleepDurationProvider;
}