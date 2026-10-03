namespace FclEx.Extensions;

partial class StringExtensions
{
    public static string Replace(this string str, Regex regex, string replacement)
    {
        return regex.Replace(str, replacement);
    }

    /// <summary>
    /// Converts all line endings in the string to LF ("\n").
    /// Treats "\r", "\r\n", and "\n" as newlines.
    /// </summary>
    public static string LineEndingToLf(this string text)
    {
        return RegexReplacer.LineEndingToLf.Replace(text);
    }

    /// <summary>
    /// Converts all line endings in the string to CRLF ("\r\n").
    /// Treats "\r", "\r\n", and "\n" as newlines.
    /// </summary>
    public static string LineEndingToCrLf(this string text)
    {
        return RegexReplacer.LineEndingToCrLf.Replace(text);
    }
}
