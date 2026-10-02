namespace FclEx.Extensions;

partial class StringExtensions
{
    /// <summary>
    /// Determines whether the string is enclosed in square brackets [ ].
    /// </summary>
    /// <param name="str">The string to check.</param>
    /// <returns>True if the string starts with '[' and ends with ']'; otherwise, false.</returns>
    public static bool IsSquareBracketed(this string str)
    {
        return str.Length >= 2 && str.StartsWith('[') && str.EndsWith(']');
    }

    /// <summary>
    /// Removes the leading and trailing square brackets from the string, if present.
    /// </summary>
    /// <param name="str">The string to process.</param>
    /// <returns>The string without surrounding square brackets, or the original string if none are found.</returns>
    public static string TrimSquareBrackets(this string str)
    {
        return str.IsSquareBracketed()
            ? str[1..^1]
            : str;
    }
}
