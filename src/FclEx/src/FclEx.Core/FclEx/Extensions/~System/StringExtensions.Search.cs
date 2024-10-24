namespace FclEx.Extensions;

static partial class StringExtensions
{
    /// <summary>Returns a value indicating whether a specified string occurs within this string, using the specified comparison rules.</summary>
    /// <param name="source"></param>
    /// <param name="value">The string to seek.</param>
    /// <param name="compareOptions"></param>
    /// <param name="compareInfo"></param>
    public static bool Contains(this string source, string value, CompareOptions compareOptions, CompareInfo? compareInfo = null)
    {
        compareInfo ??= CultureInfo.InvariantCulture.CompareInfo;
        return compareInfo.IndexOf(source, value, compareOptions) >= 0;
    }

    public static bool ContainsAny(this string source, IEnumerable<string> values, StringComparison comparison = StringComparison.Ordinal)
        => values.Any(m => source.Contains(m, comparison));

    public static bool ContainsAny(this string source, IEnumerable<string> values, CompareOptions options)
        => values.Any(m => source.Contains(m, options));

    public static bool ContainsAll(this string source, IEnumerable<string> values, StringComparison comparison = StringComparison.Ordinal)
        => values.All(m => source.Contains(m, comparison));

    public static bool ContainsAll(this string source, IEnumerable<string> values, CompareOptions options)
        => values.All(m => source.Contains(m, options));

    public static bool StartsWithAny(this string source, IEnumerable<string> values, StringComparison comparison = StringComparison.Ordinal)
        => values.Any(m => source.StartsWith(m, comparison));

    public static bool EndsWithAny(this string source, IEnumerable<string> values, StringComparison comparison = StringComparison.Ordinal)
        => values.Any(m => source.EndsWith(m, comparison));

    public static bool ContainsAnyIgnoreCase(this string source, IEnumerable<string> values)
        => source.ContainsAny(values, StringComparison.OrdinalIgnoreCase);

    public static bool ContainsIgnoreCase(this string source, string value)
        => source.Contains(value, StringComparison.OrdinalIgnoreCase);

    public static bool EqualsIgnoreCase(this string source, string value)
        => source.Equals(value, StringComparison.OrdinalIgnoreCase);

#if NETSTANDARD2_0
    public static bool Contains(this string source, string value, StringComparison comparison)
        => source.IndexOf(value, comparison) >= 0;
#endif
}
