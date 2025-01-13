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


    private static char[] TrimChars { get; } = { '\'', '"', ';' };

    public static string? GetMetaCharSet(string html)
    {
        if (html.IsNullOrEmpty())
            return null;

        var match = CommonWebRegexes.CharSet.Match(html);
        if (match.Success)
        {
            return match.Groups[1].Value.Trim(TrimChars);
        }
        else
        {
            return null;
        }
    }

    public static string? GetMetaRefreshUrl(string html)
    {
        if (html.IsNullOrWhiteSpace())
            return null;

        var match = CommonWebRegexes.MetaRefresh.Match(html);
        if (match.Success)
        {
            var refresh = match.Groups[1].Value;

            refresh = refresh.Replace("&#x27;", "'")
                .Replace("&#39;", "'")
                .Replace("&#x22;", "\"")
                .Replace("&#34;", "\"")
                .Trim();
            var nextMatch = CommonWebRegexes.MetaRefreshUrl.Match(refresh);
            if (nextMatch.Success)
            {
                var g = nextMatch.Groups;
                return (g[2].Value, g[3].Value).FirstNotEmpty();
            }

            return null;
        }
        else
        {
            return null;
        }
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