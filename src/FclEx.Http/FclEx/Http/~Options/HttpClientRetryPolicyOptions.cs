namespace FclEx.Http;

/// <summary>
/// Options for the Polly policies registered on FclEx HTTP clients.
/// The configured policies cover transient HTTP responses, per-attempt execution timeout, connect-timeout failures, and IO exceptions.
/// </summary>
public record HttpClientRetryPolicyOptions
{
    /// <summary>
    /// Default retry delay provider. It returns <see cref="TimeSpan.Zero"/>, so retries are immediate unless <see cref="SleepDurationProvider"/> is replaced.
    /// </summary>
    public static readonly SleepDurationProvider DefaultSleepDurationProvider = retryAttempt => TimeSpan.Zero;

    /// <summary>
    /// Timeout applied to each individual send execution through Polly's timeout policy.
    /// This is separate from <see cref="HttpClient.Timeout"/> and from <see cref="HttpRequest.TotalTimeout"/>.
    /// </summary>
    public TimeSpan ExecutionTimeout { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Number of retries used by the HTTP, connect-timeout, and IO retry policies.
    /// The original attempt is not counted as a retry.
    /// </summary>
    public int RetryCount { get; set; } = 2;

    /// <summary>
    /// Indicates whether retry policy registration should raise <see cref="HttpClient.Timeout"/> when it is shorter than the maximum expected retry window.
    /// The computed window includes the original execution, all retry executions, retry delays, and a small extra allowance.
    /// </summary>
    public bool AutoUpdateTotalTimeout { get; set; } = true;

    /// <summary>
    /// Provides the delay before each retry attempt.
    /// The retry attempt number passed to the delegate starts at 1.
    /// </summary>
    public SleepDurationProvider SleepDurationProvider { get; set; } = DefaultSleepDurationProvider;
}
