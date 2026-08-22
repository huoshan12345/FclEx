namespace FclEx.Http;

/// <summary>
/// Compares <see cref="NetworkCredential"/> instances by user name, password, and domain.
/// </summary>
public class NetworkCredentialEqualityComparer : IEqualityComparer<NetworkCredential>
{
    /// <summary>
    /// A shared comparer instance.
    /// </summary>
    public static readonly NetworkCredentialEqualityComparer Instance = new();

    public bool Equals(NetworkCredential? x, NetworkCredential? y)
    {
        if (Comparer.TryEquals(x, y, out var result))
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
