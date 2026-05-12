namespace FclEx.Http;

public class UriOriginEqualityComparer : IEqualityComparer<Uri>
{
    public bool Equals(Uri? x, Uri? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null) return false;
        if (y is null) return false;
        if (x.GetType() != y.GetType()) return false;

        return string.Equals(x.Host, y.Host, StringComparison.OrdinalIgnoreCase)
               && x.Port == y.Port
               && string.Equals(x.Scheme, y.Scheme, StringComparison.OrdinalIgnoreCase);
    }

    public int GetHashCode(Uri obj)
    {
        return HashCode.Combine(obj.Host.ToLower(), obj.Port, obj.Scheme.ToLower());
    }

    public static readonly UriOriginEqualityComparer Instance = new();
}