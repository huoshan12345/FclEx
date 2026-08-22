namespace FclEx.Http;

/// <summary>
/// Helpers for parsing HTML and extracting common metadata.
/// </summary>
public static class HtmlHelper
{
    private static readonly char[] TrimChars = ['\'', '"', ';', ' '];

    /// <summary>
    /// Extracts the first charset declaration from supported HTML meta tag shapes.
    /// Surrounding quotes, semicolons, and spaces are trimmed from the returned charset.
    /// </summary>
    public static string? GetMetaCharSet(string html)
    {
        if (html.IsNullOrEmpty())
            return null;

        foreach (var regex in Regexes.CharSet)
        {
            var match = regex.Match(html);
            if (match.Success)
                return match.Groups["charset"].Value.Trim(TrimChars);
        }

        return null;
    }
}
