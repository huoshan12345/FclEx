namespace FclEx.Http;

/// <summary>
/// Helpers for parsing HTML and extracting common metadata.
/// </summary>
public static class HtmlHelper
{
    private static readonly HtmlParser DefaultHtmlParser = new();

    /// <summary>
    /// Parses HTML text into an AngleSharp document using the shared parser instance.
    /// </summary>
    public static IHtmlDocument Parse(string html)
    {
        return DefaultHtmlParser.ParseDocument(html);
    }

    /// <summary>
    /// Parses HTML text asynchronously into an AngleSharp document using the shared parser instance.
    /// </summary>
    public static Task<IHtmlDocument> ParseAsync(string html)
    {
        return DefaultHtmlParser.ParseDocumentAsync(html);
    }

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

    /// <summary>
    /// Parses HTML and returns the document body's text content.
    /// </summary>
    public static string? GetTextContent(string str)
    {
        var html = DefaultHtmlParser.ParseDocument(str);
        return html.Body?.TextContent;
    }

    /// <summary>
    /// Returns the parsed document body's text content, or the original string when no body text can be produced.
    /// </summary>
    public static string RemoveHtmlTags(string str)
    {
        return GetTextContent(str) ?? str;
    }
}
