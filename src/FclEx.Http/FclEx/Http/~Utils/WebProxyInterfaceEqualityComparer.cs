namespace FclEx.Http;

public class WebProxyInterfaceEqualityComparer : IEqualityComparer<IWebProxy>
{
    public static readonly WebProxyInterfaceEqualityComparer Instance = new();

    private static IEqualityComparer<WebProxy> WebProxyComparer
        => WebProxyEqualityComparer.Instance;

    public bool Equals(IWebProxy? x, IWebProxy? y)
    {
        if (ComparerHelper.TryEquals(x, y, out var result))
            return result.Value;

        if (x is WebProxy wx && y is WebProxy wy)
            return WebProxyComparer.Equals(wx, wy);

        return false;
    }

    public int GetHashCode(IWebProxy obj)
    {
        if (obj is WebProxy webProxy)
            return WebProxyComparer.GetHashCode(webProxy);

        return obj.GetHashCode();
    }
}