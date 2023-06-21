namespace FclEx.Http;

public class SocketsHttpHandlerOptions
{
    public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(10);
    public IPVersionOption IpVersionOption { get; set; } = IPVersionOption.PreferIPv4;
    public bool AllowAutoRedirect { get; set; } = true;
    public DecompressionMethods AutomaticDecompression { get; set; } = DecompressionMethods.All;
    /// <summary>
    /// The connection is reestablished periodically to reflect the DNS or other network changes.
    /// </summary>
    public TimeSpan PooledConnectionLifetime { get; set; } = TimeSpan.FromMinutes(1);
    public TimeSpan PooledConnectionIdleTimeout { get; set; } = TimeSpan.FromMinutes(1);
    public IWebProxy? Proxy { get; set; }

    public static readonly SocketsHttpHandlerOptions Default = new();
}