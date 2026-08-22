namespace FclEx.Http;

public class SocketsHttpHandlerOptionsEqualityComparer : IEqualityComparer<SocketsHttpHandlerOptions>
{
    public static readonly SocketsHttpHandlerOptionsEqualityComparer Instance = new();

    public static IEqualityComparer<IWebProxy> WebProxyComparer
        => WebProxyInterfaceEqualityComparer.Instance;

    public bool Equals(SocketsHttpHandlerOptions? x, SocketsHttpHandlerOptions? y)
    {
        if (Comparer.TryEquals(x, y, out var result))
            return result.Value;

        return x.ConnectTimeout.Equals(y.ConnectTimeout)
               && x.IPVersionPolicy == y.IPVersionPolicy
               && x.AllowAutoRedirect == y.AllowAutoRedirect
               && x.AutomaticDecompression == y.AutomaticDecompression
               && x.EnableMultipleHttp2Connections == y.EnableMultipleHttp2Connections
               && x.PooledConnectionLifetime.Equals(y.PooledConnectionLifetime)
               && x.PooledConnectionIdleTimeout.Equals(y.PooledConnectionIdleTimeout)
               && x.DisableServerCertificateValidation == y.DisableServerCertificateValidation
               && WebProxyComparer.Equals(x.Proxy!, y.Proxy!);
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
        hash.Add(WebProxyComparer.GetHashCodeOrDefault(obj.Proxy));
        return hash.ToHashCode();
    }
}
