using System.Collections.Specialized;
using AngleSharp.Html.Dom;
using FclEx;
using FclEx.Utils;

namespace AngleSharp.Dom
{
    public class HtmlAnchor
    {
        public HtmlAnchor(IHtmlAnchorElement element)
        {
            Element = element;
            var (l, r) = element.Href.SplitTwo("?");
            Query = r.ParseQueryString();
            Path = l;
        }

        public IHtmlAnchorElement Element { get; }
        public string Path { get; }
        public NameValueCollection Query { get; }

        public void Deconstruct(out string text, out string path, out NameValueCollection query, out string title, out IHtmlAnchorElement element)
        {
            element = Element;
            text = Element?.TextContent ?? "";
            title = Element?.Title ?? "";
            query = Query;
            path = Path;
        }

        public static readonly HtmlAnchor Empty = new(HtmlHelper.Parse("<a></a>").QuerySelector<IHtmlAnchorElement>("a")!);
    }
}
