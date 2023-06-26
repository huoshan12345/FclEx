namespace FclEx.Http;

public record SocketsHttpHandlerOptions
{
    public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(5);
    public IPVersionPolicy IPVersionPolicy { get; set; } = IPVersionPolicy.PreferIPv4;
    public bool AllowAutoRedirect { get; set; } = true;
    public DecompressionMethods AutomaticDecompression { get; set; } = DecompressionMethods.All;
    public IWebProxy? Proxy { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates whether additional HTTP/2 connections can be established to the same server
    /// when the maximum of concurrent streams is reached on all existing connections.
    /// </summary>
    public bool EnableMultipleHttp2Connections { get; set; } = false;

    /// <summary>
    /// Gets or sets how long a connection can be in the pool to be considered reusable. <br/>
    /// The connection is reestablished periodically to reflect the DNS or other network changes.
    /// </summary>
    public TimeSpan PooledConnectionLifetime { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Gets or sets how long a connection can be idle in the pool to be considered reusable.
    /// </summary>
    public TimeSpan PooledConnectionIdleTimeout { get; set; } = TimeSpan.FromMinutes(2);

    public static readonly SocketsHttpHandlerOptions Default = new();
}