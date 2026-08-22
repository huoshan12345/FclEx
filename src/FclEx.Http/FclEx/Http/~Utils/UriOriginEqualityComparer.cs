namespace FclEx.Http;

/// <summary>
/// Compares URIs by scheme, host, and port only.
/// </summary>
/// <remarks>Path, query, fragment, user info, and host/scheme casing are ignored.</remarks>
public class UriOriginEqualityComparer : IEqualityComparer<Uri>
{
    /// <summary>
    /// A shared comparer instance.
    /// </summary>
    public static readonly UriOriginEqualityComparer Instance = new();

    public bool Equals(Uri? x, Uri? y)
    {
        if (Comparer.TryEquals(x, y, out var result))
            return result.Value;

        return string.Equals(x.Host, y.Host, StringComparison.OrdinalIgnoreCase)
               && x.Port == y.Port
               && string.Equals(x.Scheme, y.Scheme, StringComparison.OrdinalIgnoreCase);
    }

    public int GetHashCode(Uri obj)
    {
        return HashCode.Combine(obj.Host.ToLower(), obj.Port, obj.Scheme.ToLower());
    }
}
