namespace FclEx.Http;

public class IWebProxyEqualityComparer : IEqualityComparer<IWebProxy>
{
    public bool Equals(IWebProxy? x, IWebProxy? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
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

    public static readonly IWebProxyEqualityComparer Instance = new();
}