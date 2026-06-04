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