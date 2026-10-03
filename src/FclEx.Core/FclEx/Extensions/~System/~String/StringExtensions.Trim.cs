namespace FclEx.Extensions;

public static partial class StringExtensions
{
    [return: NotNullIfNotNull(nameof(source))]
    public static string? TrimStart(
        this string? source,
        string? trimString,
        bool onlyOnce = false,
        StringComparison comparison = StringComparison.Ordinal)
    {
        if (source.IsNullOrEmpty() || trimString.IsNullOrEmpty())
            return source;

        ReadOnlySpan<char> span = source;
        ReadOnlySpan<char> trim = trimString;

        var offset = 0;
        while (span[offset..].StartsWith(trim, comparison))
        {
            offset += trim.Length;
            if (onlyOnce)
                break;
        }

        return offset == 0 ? source : source[offset..];
    }

    [return: NotNullIfNotNull(nameof(source))]
    public static string? TrimEnd(
        this string? source,
        string? trimString,
        bool onlyOnce = false,
        StringComparison comparison = StringComparison.Ordinal)
    {
        if (source.IsNullOrEmpty() || trimString.IsNullOrEmpty())
            return source;

        ReadOnlySpan<char> span = source;
        ReadOnlySpan<char> trim = trimString;

        var length = span.Length;
        while (span[..length].EndsWith(trim, comparison))
        {
            length -= trim.Length;
            if (onlyOnce)
                break;
        }

        return length == source.Length ? source : source[..length];
    }

    [return: NotNullIfNotNull(nameof(source))]
    public static string? Trim(
        this string? source,
        string? trimString,
        bool onlyOnce = false,
        StringComparison comparison = StringComparison.Ordinal)
    {
        return source
            .TrimStart(trimString, onlyOnce, comparison)
            .TrimEnd(trimString, onlyOnce, comparison);
    }

    public static string SkipUntil(
        this string source,
        string separator,
        bool skipSeparator = true,
        bool untilLast = false,
        StringComparison comparison = StringComparison.Ordinal)
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

    public static string SkipBefore(
        this string source,
        string separator,
        bool untilLast = false,
        StringComparison comparison = StringComparison.Ordinal)
    {
        return source.SkipUntil(separator, false, untilLast, comparison);
    }

    public static string TakeUntil(
        this string source,
        string separator,
        bool includeSeparator = true,
        bool untilLast = false,
        StringComparison comparison = StringComparison.Ordinal)
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

    public static string TakeBefore(
        this string source,
        string separator,
        bool untilLast = false,
        StringComparison comparison = StringComparison.Ordinal)
    {
        return source.TakeUntil(separator, false, untilLast, comparison);
    }
}