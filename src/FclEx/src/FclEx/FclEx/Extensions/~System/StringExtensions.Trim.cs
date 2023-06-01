using System.Diagnostics.CodeAnalysis;

namespace FclEx.Extensions;

public static partial class StringExtensions
{
    [return: NotNullIfNotNull("target")]
    public static string? TrimStart(this string? target, string? trimString)
    {
        if (string.IsNullOrEmpty(target) || string.IsNullOrEmpty(trimString))
            return target;

        var result = target;
        while (result.StartsWith(trimString))
        {
            result = result[trimString.Length..];
        }
        return result;
    }

    [return: NotNullIfNotNull("target")]
    public static string? TrimEnd(this string? target, string? trimString)
    {
        if (string.IsNullOrEmpty(target) || string.IsNullOrEmpty(trimString))
            return target;

        var result = target;
        while (result.EndsWith(trimString))
        {
            result = result[..^trimString.Length];
        }
        return result;
    }

    public static string SkipUntil(this string source, string separator, bool skipSeparator = true, StringComparison comparison = StringComparison.Ordinal, bool untilLast = false)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (separator == null) throw new ArgumentNullException(nameof(separator));

        var location = untilLast
            ? source.LastIndexOf(separator, comparison)
            : source.IndexOf(separator, comparison);

        if (location < 0)
            return source;

        if (skipSeparator)
            location += separator.Length;

        return source[location..];
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
}