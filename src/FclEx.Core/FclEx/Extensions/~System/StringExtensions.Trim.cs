namespace FclEx.Extensions;

public static partial class StringExtensions
{
    [return: NotNullIfNotNull(nameof(source))]
    public static string? TrimStart(this string? source, string? trimString, bool onlyOnce = false)
    {
        if (source.IsNullOrEmpty() || trimString.IsNullOrEmpty())
            return source;

        var result = source;
        while (result.StartsWith(trimString))
        {
            result = result[trimString.Length..];
            if (onlyOnce)
                break;
        }
        return result;
    }

    [return: NotNullIfNotNull(nameof(source))]
    public static string? TrimEnd(this string? source, string? trimString, bool onlyOnce = false)
    {
        if (source.IsNullOrEmpty() || trimString.IsNullOrEmpty())
            return source;

        var result = source;
        while (result.EndsWith(trimString))
        {
            result = result[..^trimString.Length];
            if (onlyOnce)
                break;
        }
        return result;
    }

    public static string SkipUntil(this string source, string separator, bool skipSeparator = true, StringComparison comparison = StringComparison.Ordinal, bool untilLast = false)
    {
        Check.NotNull(source);
        Check.NotNull(separator);

        var location = untilLast
            ? source.LastIndexOf(separator, comparison)
            : source.IndexOf(separator, comparison);

        if (location < 0)
            return source;

        if (skipSeparator)
            location += separator.Length;

        return source[location..];
    }

    public static string SkipBefore(this string source, string separator, StringComparison comparison = StringComparison.Ordinal, bool untilLast = false)
    {
        return source.SkipUntil(separator, false, comparison, untilLast);
    }

    public static string TakeUntil(this string source, string separator, bool includeSeparator = true, StringComparison comparison = StringComparison.Ordinal, bool untilLast = false)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (separator == null) throw new ArgumentNullException(nameof(separator));

        var location = untilLast
            ? source.LastIndexOf(separator, comparison)
            : source.IndexOf(separator, comparison);

        if (location < 0)
            return source;

        if (includeSeparator)
            location += separator.Length;

        return source[..location];
    }

    public static string TakeBefore(this string source, string separator, StringComparison comparison = StringComparison.Ordinal, bool untilLast = false)
    {
        return source.TakeUntil(separator, false, comparison, untilLast);
    }
}