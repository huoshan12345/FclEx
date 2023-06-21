namespace FclEx.Http;

public class SocketsHttpHandlerOptionsEqualityComparer : IEqualityComparer<SocketsHttpHandlerOptions>
{
    public static readonly IEqualityComparer<IWebProxy> ProxyEqualityComparer = IWebProxyEqualityComparer.Instance;
    public static readonly SocketsHttpHandlerOptionsEqualityComparer Instance = new();

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
               && ProxyEqualityComparer.Equals(x.Proxy, y.Proxy);
    }

    public int GetHashCode(SocketsHttpHandlerOptions obj)
    {
        var proxyCode = obj.Proxy is null 
            ? 0 
            : ProxyEqualityComparer.GetHashCode(obj.Proxy);

        return HashCode.Combine(
            obj.ConnectTimeout,
            (int)obj.IpVersionOption,
            obj.AllowAutoRedirect,
            (int)obj.AutomaticDecompression,
            obj.PooledConnectionLifetime,
            obj.PooledConnectionIdleTimeout,
            proxyCode);
    }
}