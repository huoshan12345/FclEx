namespace FclEx.Http;

public readonly struct HtmlActionContext
{
    public HtmlActionContext(HttpResponse response, string html, string? path)
    {
        Response = response;
        Html = html;
        Path = path;
        Element = HtmlHelper.Parse(html).DocumentElement;
        ResultElements = path == null
            ? Element.Yield().ToCollection()
            : Element.QuerySelectorAll(path)!;
    }

    public HttpResponse Response { get; }
    public string? Path { get; }
    public string Html { get; }
    public IElement Element { get; }
    public IHtmlCollection<IElement> ResultElements { get; }
    public IElement? ResultElement => ResultElements.FirstOrDefault();
}