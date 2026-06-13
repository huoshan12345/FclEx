namespace AngleSharp.Dom;

/// <summary>
/// Wraps an AngleSharp anchor element and exposes the anchor href as a path plus parsed query parameters.
/// </summary>
public class HtmlAnchor
{
    /// <summary>
    /// Creates a wrapper for an anchor element.
    /// The element's <see cref="IHtmlAnchorElement.Href"/> is split at the first question mark and the query part is parsed into <see cref="UriParams"/>.
    /// </summary>
    public HtmlAnchor(IHtmlAnchorElement element)
    {
        Element = element;
        var (l, r) = element.Href.Partition("?");
        Query = UriParams.Parse(r);
        Path = l;
    }

    /// <summary>
    /// The wrapped anchor element.
    /// </summary>
    public IHtmlAnchorElement Element { get; }

    /// <summary>
    /// The href value before the first question mark.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Query parameters parsed from the href value after the first question mark.
    /// </summary>
    public UriParams Query { get; }

    /// <summary>
    /// Deconstructs the wrapper into text, path, query parameters, title, and the underlying anchor element.
    /// </summary>
    public void Deconstruct(out string text, out string path, out UriParams query, out string title, out IHtmlAnchorElement element)
    {
        element = Element;
        text = Element?.TextContent ?? "";
        title = Element?.Title ?? "";
        query = Query;
        path = Path;
    }

    /// <summary>
    /// Empty anchor wrapper backed by a parsed <c>&lt;a&gt;</c> element.
    /// </summary>
    public static readonly HtmlAnchor Empty = new(HtmlHelper.Parse("<a></a>").QuerySelector<IHtmlAnchorElement>("a")!);
}
