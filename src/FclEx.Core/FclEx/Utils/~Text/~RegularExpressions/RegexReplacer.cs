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
    public static readonly RegexReplacer LF_TO_CRLF = new("(?<!\r)\n", "\r\n");
    public static readonly RegexReplacer CRLF_TO_LF = new("[\r\n]+", "\n");

    public RegexReplacer(string pattern, string replacement, RegexOptions options = RegexOptions.Compiled)
        : this(new Regex(pattern, options), m => replacement) { }

    public string Replace(string input)
    {
        return Regex.Replace(input, Evaluator);
    }
}