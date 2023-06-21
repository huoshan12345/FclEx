namespace FclEx.Http;

public class SocketsHttpHandlerOptions : IEqualityComparer<SocketsHttpHandlerOptions>
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

    public bool Equals(SocketsHttpHandlerOptions? x, SocketsHttpHandlerOptions? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.ConnectTimeout.Equals(y.ConnectTimeout)
               && x.IpVersionOption == y.IpVersionOption
               && x.AllowAutoRedirect == y.AllowAutoRedirect
               && x.AutomaticDecompression == y.AutomaticDecompression
               && x.PooledConnectionLifetime.Equals(y.PooledConnectionLifetime)
               && x.PooledConnectionIdleTimeout.Equals(y.PooledConnectionIdleTimeout)
               && IWebProxyEqualityComparer.Instance.Equals(x.Proxy, y.Proxy);
    }

    public int GetHashCode(SocketsHttpHandlerOptions obj)
    {
        return HashCode.Combine(obj.ConnectTimeout,
            (int)obj.IpVersionOption,
            obj.AllowAutoRedirect,
            (int)obj.AutomaticDecompression,
            obj.PooledConnectionLifetime,
            obj.PooledConnectionIdleTimeout,
            obj.Proxy);
    }
}