using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

namespace FclEx.Utils;

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
}