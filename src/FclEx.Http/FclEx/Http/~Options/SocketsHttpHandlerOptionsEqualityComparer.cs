namespace FclEx.Http;

public class SocketsHttpHandlerOptionsEqualityComparer : IEqualityComparer<SocketsHttpHandlerOptions>
{
    public static readonly IEqualityComparer<IWebProxy> ProxyEqualityComparer = WebProxyInterfaceEqualityComparer.Instance;
    public static readonly SocketsHttpHandlerOptionsEqualityComparer Instance = new();

    public bool Equals(SocketsHttpHandlerOptions? x, SocketsHttpHandlerOptions? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null) return false;
        if (y is null) return false;
        if (x.GetType() != y.GetType()) return false;

        return x.ConnectTimeout.Equals(y.ConnectTimeout)
               && x.IPVersionPolicy == y.IPVersionPolicy
               && x.AllowAutoRedirect == y.AllowAutoRedirect
               && x.AutomaticDecompression == y.AutomaticDecompression
               && x.EnableMultipleHttp2Connections == y.EnableMultipleHttp2Connections
               && x.PooledConnectionLifetime.Equals(y.PooledConnectionLifetime)
               && x.PooledConnectionIdleTimeout.Equals(y.PooledConnectionIdleTimeout)
               && x.DisableServerCertificateValidation == y.DisableServerCertificateValidation
               && ProxyEqualityComparer.Equals(x.Proxy!, y.Proxy!);
    }

    public int GetHashCode(SocketsHttpHandlerOptions obj)
    {
        var hash = new HashCode();
        hash.Add(obj.ConnectTimeout);
        hash.Add(obj.IPVersionPolicy);
        hash.Add(obj.AllowAutoRedirect);
        hash.Add(obj.AutomaticDecompression);
        hash.Add(obj.EnableMultipleHttp2Connections);
        hash.Add(obj.PooledConnectionLifetime);
        hash.Add(obj.PooledConnectionIdleTimeout);
        hash.Add(obj.DisableServerCertificateValidation);
        hash.Add(ProxyEqualityComparer.GetHashCodeOrDefault(obj.Proxy));
        return hash.ToHashCode();
    }
}
