namespace FclEx.Http;

/// <summary>
/// Options used by <see cref="HttpMessageHandler.CreateSocketsHttpHandler"/> when constructing a <see cref="SocketsHttpHandler"/>.
/// The type is a record so callers can use <c>with</c> expressions when deriving client-specific handler settings.
/// </summary>
public record SocketsHttpHandlerOptions // use record so that with expression can be used
{
    /// <summary>
    /// Timeout for establishing a socket connection before the send attempt fails.
    /// </summary>
    public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Address-family preference used by the custom connect callback.
    /// This lets callers prefer IPv4 or IPv6 without changing the request URI.
    /// </summary>
    public IPVersionPolicy IPVersionPolicy { get; set; } = IPVersionPolicy.PreferIPv4;

    /// <summary>
    /// Indicates whether the underlying handler should automatically follow redirects.
    /// FclEx services usually disable this and perform their own redirect handling so redirect history, cookies, and downgrade rules can be controlled by <see cref="HttpRequest"/>.
    /// </summary>
    public bool AllowAutoRedirect { get; set; } = true;

    /// <summary>
    /// Response decompression algorithms enabled on the handler.
    /// Older target frameworks default to GZip only; newer targets use all algorithms supported by <see cref="DecompressionMethods.All"/>.
    /// </summary>
    public DecompressionMethods AutomaticDecompression { get; set; } =
#if !NET5_0_OR_GREATER
        DecompressionMethods.GZip;
#else
        DecompressionMethods.All;
#endif

    /// <summary>
    /// Proxy assigned to <see cref="HttpClientHandler.Proxy"/>.
    /// A <see langword="null"/> value leaves the handler without an explicit proxy.
    /// </summary>
    public IWebProxy? Proxy { get; set; }

    /// <summary>
    /// Indicates whether additional HTTP/2 connections can be established to the same server
    /// when the maximum of concurrent streams is reached on all existing connections.
    /// </summary>
    public bool EnableMultipleHttp2Connections { get; set; } = false;

    /// <summary>
    /// How long a connection can remain in the pool and still be considered reusable. <br/>
    /// The connection is reestablished periodically to reflect the DNS or other network changes.
    /// </summary>
    public TimeSpan PooledConnectionLifetime { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// How long an idle pooled connection can remain reusable before the handler closes it.
    /// </summary>
    public TimeSpan PooledConnectionIdleTimeout { get; set; } = TimeSpan.FromMinutes(2);

    /// <summary>
    /// Whether to disable server certificate validation.
    /// This should only be enabled in development or trusted test environments.
    /// </summary>
    public bool DisableServerCertificateValidation { get; set; }
}
