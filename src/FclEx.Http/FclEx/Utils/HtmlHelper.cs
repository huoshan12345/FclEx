using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using FclEx.Web.Models;

namespace FclEx.Utils
{
    public static class HtmlHelper
    {
        private static readonly HtmlParser DefaultHtmlParser = new HtmlParser();

        public static SubmitInfo? GetFormSubmitInfo(Uri htmlUrl, IHtmlElement html, string formSelector)
        {
            var form = html.QuerySelector(formSelector);
            if (form == null) return null;

            var info = new SubmitInfo(new Uri(form.GetAttribute("action"), UriKind.RelativeOrAbsolute));

            if (!info.SubmitUrl.IsAbsoluteUri)
                info.SubmitUrl = new Uri(htmlUrl, info.SubmitUrl);

            foreach (var input in form.QuerySelectorAll("input").OfType<IHtmlInputElement>().Where(m => m.Type == "hidden"))
            {
                var name = input.GetAttribute("name");
                if (name.IsNullOrEmpty())
                    continue;
                info.Paras[name] = input.GetAttribute("value");
            }

            return info;
        }

        public static IHtmlDocument Parse(string html)
        {
            return DefaultHtmlParser.ParseDocument(html);
        }

        public static Task<IHtmlDocument> ParseAsync(string html)
        {
            return DefaultHtmlParser.ParseDocumentAsync(html);
        }

        public static IHtmlElement ParseBody(string html)
        {
            return DefaultHtmlParser.ParseDocument(html).Body;
        }

        public static async Task<IHtmlElement> ParseBodyAsync(string html)
        {
            return (await ParseAsync(html).DonotCapture()).Body;
        }
    }
}
