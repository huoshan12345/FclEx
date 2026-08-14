using static FclEx.Extensions.SeparatorLocationOption;

namespace FclEx.Extensions;

public enum SeparatorLocationOption
{
    /// <summary>
    /// Excludes the separator from both parts.
    /// Both the left and right parts will contain only the string content without the separator.
    /// </summary>
    None = 0,
    /// <summary>
    /// Includes the separator in the left part of the split.
    /// The right part will contain only the remaining portion of the string.
    /// </summary>
    Left,
    /// <summary>
    /// Includes the separator in the right part of the split.
    /// The left part will contain only the portion before the separator.
    /// </summary>
    Right,
    /// <summary>
    /// Includes the separator in both parts.
    /// Both the left and right parts will contain the separator in their respective portions.
    /// </summary>
    Both,
}

partial class StringExtensions
{
    private static readonly string[] NewLineSeparators = ["\r\n", "\r", "\n"];

    public static string[] SplitToLines(this string? str, StringSplitOptions options)
    {
        return SplitToLines(str, (int)options, nameof(options));
    }

    public static string[] SplitToLines(this string? str, SplitOptions options = SplitOptions.TrimAndRemoveEmpty)
    {
        return SplitToLines(str, (int)options, nameof(options));
    }

    private static string[] SplitToLines(string? str, int options, string parameterName)
    {
        const int removeEmptyEntries = (int)SplitOptions.RemoveEmptyEntries;
        const int trimEntries = (int)SplitOptions.TrimEntries;
        const int supportedOptions = removeEmptyEntries | trimEntries;

        if ((options & ~supportedOptions) != 0)
            throw new ArgumentOutOfRangeException(parameterName, options, null);

        if (str.IsNullOrEmpty())
            return [];

        var lines = str.Split(NewLineSeparators, StringSplitOptions.None);
        if ((options & trimEntries) != 0)
        {
            for (var i = 0; i < lines.Length; i++)
                lines[i] = lines[i].Trim();
        }

        return (options & removeEmptyEntries) != 0
            ? lines.Where(line => line.Length > 0).ToArray()
            : lines;
    }

    /// <summary>
    /// Splits the given string into two parts based on the specified separator.
    /// </summary>
    /// <param name="source">The string to be split.</param>
    /// <param name="separator">The character or substring used as the separator.</param>
    /// <param name="option">The option that determines where the separator should be included in the resulting parts.</param>
    /// <param name="fromRight">Indicates whether to search for the separator starting from the right side of the input string.</param>
    /// <returns>
    /// A tuple containing the two parts of the string. The first part represents 
    /// the content before the separator, and the second part represents the content 
    /// after the separator, with the separator included according to the specified option.
    /// If <paramref name="fromRight"/> is true, the search will start from the end of the string.
    /// </returns>
    public static (string Left, string Right) Partition(this string source, string separator, SeparatorLocationOption option = None, bool fromRight = false)
    {
        if (source.IsNullOrEmpty())
            return ("", "");

        if (separator.IsNullOrEmpty())
            return (source, "");

        var index = fromRight
            ? source.LastIndexOf(separator, StringComparison.Ordinal)
            : source.IndexOf(separator, StringComparison.Ordinal);

        if (index < 0)
            return (source, "");

        var sepEndIndex = index + separator.Length;

        return option switch
        {
            None => (source[..index], source[sepEndIndex..]),
            Left => (source[..sepEndIndex], source[sepEndIndex..]),
            Right => (source[..index], source[index..]),
            Both => (source[..sepEndIndex], source[index..]),
            _ => throw new ArgumentOutOfRangeException(nameof(option), option, null)
        };
    }
}
