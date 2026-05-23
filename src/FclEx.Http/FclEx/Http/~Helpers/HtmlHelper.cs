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


    private static readonly char[] TrimChars = ['\'', '"', ';'];

    public static string? GetMetaCharSet(string html)
    {
        if (html.IsNullOrEmpty())
            return null;

        var match = Regexes.CharSet.Match(html);
        return match.Success
            ? match.Groups[1].Value.Trim(TrimChars)
            : null;
    }

    public static string? GetMetaRefreshUrl(string html)
    {
        if (html.IsNullOrWhiteSpace())
            return null;

        var match = Regexes.MetaRefresh.Match(html);
        // ReSharper disable once InvertIf
        if (match.Success)
        {
            var refresh = match.Groups[1].Value;

            refresh = refresh.Replace("&#x27;", "'")
                .Replace("&#39;", "'")
                .Replace("&#x22;", "\"")
                .Replace("&#34;", "\"")
                .Trim();

            var nextMatch = Regexes.MetaRefreshUrl.Match(refresh);
            // ReSharper disable once InvertIf
            if (nextMatch.Success)
            {
                var g = nextMatch.Groups;
                return (g[2].Value, g[3].Value).FirstNotEmpty();
            }
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