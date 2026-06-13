namespace FclEx;

/// <summary>
/// Adds HTTP-related regular expressions to the shared <see cref="Regexes"/> holder.
/// </summary>
public static class RegexesExtensions
{
    private static readonly Regex _emailCheck = new(@"[\da-zA-Z]+@[\da-zA-Z]+[\.][\da-zA-Z]{2,5}", RegexOptions.Compiled);

    // Supports common charset declarations such as:
    // <meta charset="utf-8">
    // <meta charset='gb2312'>
    // <meta charset=utf-8>
    // <meta http-equiv="Content-Type" content="text/html; charset=utf-8">
    // <meta content='text/html; charset=gb2312' http-equiv='Content-Type'>
    private static readonly IReadOnlyList<Regex> _charSet =
    [
        new("""<meta\b[^>]*\bcharset\s*=\s*(?:"(?<charset>[^"]+)"|'(?<charset>[^']+)'|(?<charset>[^\s"'/>;]+))""", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new("""<meta\b[^>]*\bcontent\s*=\s*(?:"[^"]*?\bcharset\s*=\s*(?<charset>[^"\s;]+)[^"]*"|'[^']*?\bcharset\s*=\s*(?<charset>[^'\s;]+)[^']*')""", RegexOptions.Compiled | RegexOptions.IgnoreCase),
    ];

    extension(Regexes)
    {
        /// <summary>
        /// Regular expression used for a lightweight email-shape check.
        /// </summary>
        public static Regex EmailCheck => _emailCheck;

        /// <summary>
        /// Regular expressions used to extract charset declarations from HTML meta tags.
        /// </summary>
        public static IReadOnlyList<Regex> CharSet => _charSet;
    }
}
