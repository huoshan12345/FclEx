using AngleSharp;
using AngleSharp.Html;
using AngleSharp.Html.Dom;

namespace FclEx.Http;

public static class ConsoleExtensions
{
    private static readonly IMarkupFormatter _formatter = new MinifyMarkupFormatter();

    extension(Console)
    {
        public static void WriteLineIfNotTesting(IHtmlDocument document, IMarkupFormatter? formatter = null)
        {
            if (Environment.IsUnderTest == false)
                Console.WriteLine(document.ToHtml(formatter ?? _formatter));
        }
    }
}