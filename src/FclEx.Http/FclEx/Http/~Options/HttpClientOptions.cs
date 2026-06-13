namespace FclEx.Http;

/// <summary>
/// Options used when creating or registering an <see cref="HttpClient"/> for FclEx HTTP services.
/// The values are applied to the client itself, its primary <see cref="SocketsHttpHandler"/>, and the retry policies added by <see cref="HttpClientBuilderExtensions"/>.
/// </summary>
public record HttpClientOptions
{
    /// <summary>
    /// Base address assigned to <see cref="HttpClient.BaseAddress"/>.
    /// Relative <see cref="HttpRequest"/> URIs are resolved against this address by <see cref="HttpClient"/>.
    /// </summary>
    public Uri? BaseAddress { get; set; }

#if NET6_0_OR_GREATER
    /// <summary>
    /// Default HTTP version negotiation policy assigned to requests created by services that use these options.
    /// Individual <see cref="HttpRequest"/> instances can still override their own version policy.
    /// </summary>
    public HttpVersionPolicy HttpVersionPolicy { get; set; } = HttpVersionPolicy.RequestVersionOrLower;

    /// <summary>
    /// Default HTTP version assigned to requests created by services that use these options.
    /// Individual <see cref="HttpRequest"/> instances can still override their own version.
    /// </summary>
    public Version HttpVersion { get; set; } = System.Net.HttpVersion.Version11;
#endif

    /// <summary>
    /// Timeout assigned to <see cref="HttpClient.Timeout"/> when a client is configured from these options.
    /// Retry policy registration may raise the client timeout further when <see cref="HttpClientRetryPolicyOptions.AutoUpdateTotalTimeout"/> is enabled.
    /// </summary>
    public TimeSpan TotalTimeout { get; set; } = TimeSpan.FromMinutes(2);

    /// <summary>
    /// Settings used to create the primary <see cref="SocketsHttpHandler"/>.
    /// </summary>
    public SocketsHttpHandlerOptions HandlerOptions { get; set; } = new();

    /// <summary>
    /// Settings used to add HTTP, timeout, connect-timeout, and IO retry policies to registered clients.
    /// </summary>
    public HttpClientRetryPolicyOptions RetryPolicyOptions { get; set; } = new();
}
