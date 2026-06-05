namespace FclEx.Http;

public class WebProxyEqualityComparer : IEqualityComparer<WebProxy>
{
    public static readonly WebProxyEqualityComparer Instance = new();

    private static IEqualityComparer<IEnumerable<string>> BypassListComparer
        => EnumerableEqualityComparer.StringOrdinalIgnoreCase;
    private static IEqualityComparer<Uri> AddressComparer
        => UriOriginEqualityComparer.Instance;
    private static IEqualityComparer<ICredentials> CredentialsComparer
        => CredentialsEqualityComparer.Instance;

    public bool Equals(WebProxy? x, WebProxy? y)
    {
        if (ComparerHelper.TryEquals(x, y, out var result))
            return result.Value;

        return AddressComparer.Equals(x.Address, y.Address)
               && BypassListComparer.Equals(x.BypassList, y.BypassList)
               && x.BypassProxyOnLocal == y.BypassProxyOnLocal
               && CredentialsComparer.Equals(x.Credentials, y.Credentials)
               && x.UseDefaultCredentials == y.UseDefaultCredentials;
    }

    public int GetHashCode(WebProxy obj)
    {
        var addressCode = AddressComparer.GetHashCodeOrDefault(obj.Address);
        var bypassListCode = BypassListComparer.GetHashCodeOrDefault(obj.BypassList);
        var credentialsCode = CredentialsComparer.GetHashCodeOrDefault(obj.Credentials);

        return HashCode.Combine(
            addressCode,
            bypassListCode,
            obj.BypassProxyOnLocal,
            credentialsCode,
            obj.UseDefaultCredentials);
    }
}