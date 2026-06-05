namespace FclEx.Http;

public class UriOriginEqualityComparer : IEqualityComparer<Uri>
{
    public static readonly UriOriginEqualityComparer Instance = new();

    public bool Equals(Uri? x, Uri? y)
    {
        if (ComparerHelper.TryEquals(x, y, out var result))
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