namespace FclEx.Http;

public static class HtmlHelper
{
    private static readonly HtmlParser DefaultHtmlParser = new();

    public static IHtmlDocument Parse(string html)
    {
        return DefaultHtmlParser.ParseDocument(html);
    }

    public static Task<IHtmlDocument> ParseAsync(string html)
    {
        return DefaultHtmlParser.ParseDocumentAsync(html);
    }


    private static readonly char[] TrimChars = ['\'', '"', ';', ' '];

    private static readonly Regex[] CharSetRegexes =
    [
        new("""<meta\b[^>]*\bcharset\s*=\s*(?:"(?<charset>[^"]+)"|'(?<charset>[^']+)'|(?<charset>[^\s"'/>;]+))""", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new("""<meta\b[^>]*\bcontent\s*=\s*(?:"[^"]*?\bcharset\s*=\s*(?<charset>[^"\s;]+)[^"]*"|'[^']*?\bcharset\s*=\s*(?<charset>[^'\s;]+)[^']*')""", RegexOptions.Compiled | RegexOptions.IgnoreCase),
    ];

    public static string? GetMetaCharSet(string html)
    {
        if (html.IsNullOrEmpty())
            return null;

        foreach (var regex in CharSetRegexes)
        {
            var match = regex.Match(html);
            if (match.Success)
                return match.Groups["charset"].Value.Trim(TrimChars);
        }

        return null;
    }

    public static string? GetTextContent(string str)
    {
        var html = DefaultHtmlParser.ParseDocument(str);
        return html.Body?.TextContent;
    }

    public static string RemoveHtmlTags(string str)
    {
        return GetTextContent(str) ?? str;
    }
}
