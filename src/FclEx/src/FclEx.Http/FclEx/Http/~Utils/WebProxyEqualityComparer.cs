namespace FclEx.Http;

public class WebProxyEqualityComparer : IEqualityComparer<WebProxy>
{
    public bool Equals(WebProxy? x, WebProxy? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;

        return SchemeAndServerEqualityComparer.Instance.Equals(x.Address, y.Address)
               && x.BypassArrayList.Equals(y.BypassArrayList)
               && x.BypassList.Equals(y.BypassList)
               && x.BypassProxyOnLocal == y.BypassProxyOnLocal
               && Equals(x.Credentials, y.Credentials)
               && x.UseDefaultCredentials == y.UseDefaultCredentials;
    }

    public int GetHashCode(WebProxy obj)
    {
        return HashCode.Combine(obj.Address, obj.BypassArrayList, obj.BypassList, obj.BypassProxyOnLocal, obj.Credentials, obj.UseDefaultCredentials);
    }

    public static readonly WebProxyEqualityComparer Instance = new();
}