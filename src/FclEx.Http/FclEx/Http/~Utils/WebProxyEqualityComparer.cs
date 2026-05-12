namespace FclEx.Http;

public class WebProxyEqualityComparer : IEqualityComparer<WebProxy>
{
    private static readonly IEqualityComparer<IEnumerable<string>> BypassListComparer
        = EnumerableEqualityComparer.StringOrdinalIgnoreCase;
    private static readonly IEqualityComparer<Uri> AddressComparer
        = UriOriginEqualityComparer.Instance;

    public bool Equals(WebProxy? x, WebProxy? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null) return false;
        if (y is null) return false;
        if (x.GetType() != y.GetType()) return false;

        return AddressComparer.Equals(x.Address, y.Address)
               && BypassListComparer.Equals(x.BypassList, y.BypassList)
               && x.BypassProxyOnLocal == y.BypassProxyOnLocal
               && Equals(x.Credentials, y.Credentials)
               && x.UseDefaultCredentials == y.UseDefaultCredentials;
    }

    public int GetHashCode(WebProxy obj)
    {
        var addressCode = AddressComparer.GetHashCodeOrDefault(obj.Address);
        var bypassListCode = BypassListComparer.GetHashCodeOrDefault(obj.BypassList);

        return HashCode.Combine(
            addressCode,
            bypassListCode,
            obj.BypassProxyOnLocal,
            obj.Credentials,
            obj.UseDefaultCredentials);
    }

    public static readonly WebProxyEqualityComparer Instance = new();
}