namespace FclEx.Extensions;

public static class StringComparisonExtensions
{
    public static StringComparer ToComparer(this StringComparison comparison)
    {
        return comparison switch
        {
            StringComparison.CurrentCulture => StringComparer.CurrentCulture,
            StringComparison.CurrentCultureIgnoreCase => StringComparer.CurrentCultureIgnoreCase,
            StringComparison.InvariantCulture => StringComparer.InvariantCulture,
            StringComparison.InvariantCultureIgnoreCase => StringComparer.InvariantCultureIgnoreCase,
            StringComparison.Ordinal => StringComparer.Ordinal,
            StringComparison.OrdinalIgnoreCase => StringComparer.OrdinalIgnoreCase,
            _ => throw new ArgumentOutOfRangeException(nameof(comparison), comparison, null)
        };
    }

#if NETSTANDARD2_0
    public static int GetHashCode(this string str, StringComparison comparison)
    {
        return comparison.ToComparer().GetHashCode(str);
    }
#endif
}