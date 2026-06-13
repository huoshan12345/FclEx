namespace FclEx.Http;

/// <summary>
/// Compares <see cref="WebProxy"/> instances by their effective proxy settings.
/// </summary>
/// <remarks>
/// Proxy addresses are compared by origin, bypass lists are compared case-insensitively, and credentials are compared
/// with <see cref="CredentialsEqualityComparer"/>.
/// </remarks>
public class WebProxyEqualityComparer : IEqualityComparer<WebProxy>
{
    /// <summary>
    /// A shared comparer instance.
    /// </summary>
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
