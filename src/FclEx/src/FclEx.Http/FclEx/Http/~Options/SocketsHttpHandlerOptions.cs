namespace FclEx.Http;

public record SocketsHttpHandlerOptions
{
    public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(5);
    public IPVersionPolicy IPVersionPolicy { get; set; } = IPVersionPolicy.PreferIPv4;
    public bool AllowAutoRedirect { get; set; } = true;
    public DecompressionMethods AutomaticDecompression { get; set; } = DecompressionMethods.All;
    /// <summary>
    /// Gets or sets how long a connection can be in the pool to be considered reusable. <br/>
    /// The connection is reestablished periodically to reflect the DNS or other network changes.
    /// </summary>
    public TimeSpan PooledConnectionLifetime { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Gets or sets how long a connection can be idle in the pool to be considered reusable.
    /// </summary>
    public TimeSpan PooledConnectionIdleTimeout { get; set; } = TimeSpan.FromMinutes(2);
    public IWebProxy? Proxy { get; set; }

    public static readonly SocketsHttpHandlerOptions Default = new();
}