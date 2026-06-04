namespace FclEx.Http;

public class WebProxyInterfaceEqualityComparer : IEqualityComparer<IWebProxy>
{
    public bool Equals(IWebProxy? x, IWebProxy? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null) return false;
        if (y is null) return false;
        if (x.GetType() != y.GetType()) return false;

        if (x is WebProxy webProxy)
        {
            return WebProxyEqualityComparer.Instance.Equals(webProxy, y.CastTo<WebProxy>());
        }

        return false;
    }

    public int GetHashCode(IWebProxy obj)
    {
        return obj is WebProxy webProxy
            ? WebProxyEqualityComparer.Instance.GetHashCode(webProxy)
            : obj.GetHashCode();
    }

    public static readonly WebProxyInterfaceEqualityComparer Instance = new();
}