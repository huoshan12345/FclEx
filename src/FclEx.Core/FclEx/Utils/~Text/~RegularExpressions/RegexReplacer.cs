namespace FclEx.Utils;

/// <summary>
/// Represents a regular expression replacer that uses a <see cref="Regex"/> 
/// to find matches in input strings and a <see cref="MatchEvaluator"/> to define 
/// the replacement logic.
/// </summary>
/// <param name="Regex">The regular expression used for matching.</param>
/// <param name="Evaluator">The function that determines how matches are replaced.</param>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public record RegexReplacer(Regex Regex, MatchEvaluator Evaluator)
{
    /// <summary>
    /// Converts all line endings in the string to CRLF ("\r\n").
    /// Treats "\r", "\r\n", and "\n" as newlines.
    /// </summary>
    public static readonly RegexReplacer LineEndingToCrLf = new(@"\r\n?|\n", "\r\n");

    /// <summary>
    /// Converts all line endings in the string to LF ("\n").
    /// Treats "\r", "\r\n", and "\n" as newlines.
    /// </summary>
    public static readonly RegexReplacer LineEndingToLf = new(@"\r\n?|\n", "\n");

    public RegexReplacer([StringSyntax(StringSyntaxAttribute.Regex, nameof(options))] string pattern, string replacement, RegexOptions options = RegexOptions.Compiled)
        : this(new Regex(pattern, options), m => replacement) { }

    public string Replace(string input)
    {
        return Regex.Replace(input, Evaluator);
    }
}