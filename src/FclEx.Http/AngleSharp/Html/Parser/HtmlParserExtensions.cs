namespace AngleSharp.Html.Parser;

public static class HtmlParserExtensions
{
    private static readonly HtmlParser DefaultHtmlParser = new();

    extension(HtmlParser)
    {
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
    }
}
