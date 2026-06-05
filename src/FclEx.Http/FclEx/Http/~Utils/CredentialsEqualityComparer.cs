namespace FclEx.Http;

public class CredentialsEqualityComparer : IEqualityComparer<ICredentials>
{
    public static readonly CredentialsEqualityComparer Instance = new();

    private static IEqualityComparer<NetworkCredential> NetworkCredentialComparer
        => NetworkCredentialEqualityComparer.Instance;

    public bool Equals(ICredentials? x, ICredentials? y)
    {
        if (ComparerHelper.TryEquals(x, y, out var result))
            return result.Value;

        if (x is NetworkCredential nx && y is NetworkCredential ny)
            return NetworkCredentialComparer.Equals(nx, ny);

        return false;
    }

    public int GetHashCode(ICredentials obj)
    {
        if (obj is NetworkCredential nc)
            return NetworkCredentialComparer.GetHashCode(nc);

        return obj.GetHashCode();
    }
}
