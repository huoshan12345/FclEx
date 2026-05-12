namespace AngleSharp.Dom;

public class HtmlAnchor
{
    public HtmlAnchor(IHtmlAnchorElement element)
    {
        Element = element;
        var (l, r) = element.Href.Partition("?");
        Query = UriParams.Parse(r);
        Path = l;
    }

    public IHtmlAnchorElement Element { get; }
    public string Path { get; }
    public UriParams Query { get; }

    public void Deconstruct(out string text, out string path, out UriParams query, out string title, out IHtmlAnchorElement element)
    {
        element = Element;
        text = Element?.TextContent ?? "";
        title = Element?.Title ?? "";
        query = Query;
        path = Path;
    }

    public static readonly HtmlAnchor Empty = new(HtmlHelper.Parse("<a></a>").QuerySelector<IHtmlAnchorElement>("a")!);
}