namespace FclEx.Http;

public class SchemeAndServerEqualityComparer : IEqualityComparer<Uri>
{
    public bool Equals(Uri? x, Uri? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return string.Equals(x.Host, y.Host, StringComparison.OrdinalIgnoreCase)
               && x.Port == y.Port
               && string.Equals(x.Scheme, y.Scheme, StringComparison.OrdinalIgnoreCase);
    }

    public int GetHashCode(Uri obj)
    {
        return HashCode.Combine(obj.Host.ToLower(), obj.Port, obj.Scheme.ToLower());
    }

    public static readonly SchemeAndServerEqualityComparer Instance = new();
}