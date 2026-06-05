namespace FclEx.Http;

public class NetworkCredentialEqualityComparer : IEqualityComparer<NetworkCredential>
{
    public static readonly NetworkCredentialEqualityComparer Instance = new();

    public bool Equals(NetworkCredential? x, NetworkCredential? y)
    {
        if (ComparerHelper.TryEquals(x, y, out var result))
            return result.Value;

        return x.UserName == y.UserName
               && x.Password == y.Password
               && x.Domain == y.Domain;
    }

    public int GetHashCode(NetworkCredential obj)
    {
        return HashCode.Combine(obj.UserName, obj.Password, obj.Domain);
    }
}
