namespace FclEx.Http;

public record HttpClientOptions
{
    public Uri? BaseAddress { get; set; }

#if NET6_0_OR_GREATER
    public HttpVersionPolicy HttpVersionPolicy { get; set; } = HttpVersionPolicy.RequestVersionOrLower;
    public Version HttpVersion { get; set; } = System.Net.HttpVersion.Version11;
#endif

    /// <summary>
    /// Will be used as <see cref="HttpClient.Timeout"/>
    /// </summary>
    public TimeSpan TotalTimeout { get; set; } = TimeSpan.FromMinutes(2);

    public SocketsHttpHandlerOptions HandlerOptions { get; set; } = new();
    public HttpClientRetryPolicyOptions RetryPolicyOptions { get; set; } = new();
}