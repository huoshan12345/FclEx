namespace FclEx.Http;

/// <summary>
/// Contains a parsed HTML response and the elements selected for an action.
/// </summary>
public readonly struct HtmlActionContext
{
    /// <summary>
    /// Initializes an HTML action context.
    /// </summary>
    /// <param name="response">The source HTTP response.</param>
    /// <param name="html">The HTML text to parse.</param>
    /// <param name="htmlSelector">The optional CSS selector. When <see langword="null"/>, the document element is selected.</param>
    /// <remarks>Invalid selectors may throw during construction.</remarks>
    public HtmlActionContext(HttpResponse response, string html, string? htmlSelector)
    {
        Response = response;
        Html = html;
        HtmlSelector = htmlSelector;
        Element = HtmlParser.Parse(html).DocumentElement;
        ResultElements = htmlSelector == null
            ? Enumerable.Repeat(Element, 1).ToCollection()
            : Element.QuerySelectorAll(htmlSelector)!;
    }

    /// <summary>
    /// Gets the source HTTP response.
    /// </summary>
    public HttpResponse Response { get; }

    /// <summary>
    /// Gets the CSS selector used to select result elements.
    /// </summary>
    public string? HtmlSelector { get; }

    /// <summary>
    /// Gets the original HTML text.
    /// </summary>
    public string Html { get; }

    /// <summary>
    /// Gets the document element.
    /// </summary>
    public IElement Element { get; }

    /// <summary>
    /// Gets the selected result elements.
    /// </summary>
    public IHtmlCollection<IElement> ResultElements { get; }

    /// <summary>
    /// Gets the first selected element, or <see langword="null"/> when no element matched.
    /// </summary>
    public IElement? ResultElement => ResultElements.FirstOrDefault();
}
