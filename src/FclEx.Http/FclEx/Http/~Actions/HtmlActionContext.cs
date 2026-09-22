namespace FclEx.Http;

/// <summary>
/// Contains a parsed HTML response and the elements selected for an action.
/// </summary>
/// <remarks>Dispose contexts created directly after use. The default HTML action pipeline disposes its context after result conversion. Copies of this struct share the same document.</remarks>
public readonly struct HtmlActionContext : IDisposable
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
        Document = HtmlParser.Parse(html);
#pragma warning disable CS0618 // Type or member is obsolete
        Element = Document.DocumentElement;
#pragma warning restore CS0618 // Type or member is obsolete
        try
        {
            ResultElements = htmlSelector == null
                ? Enumerable.Repeat(Document.DocumentElement, 1).ToCollection()
                : Document.QuerySelectorAll(htmlSelector);
        }
        catch
        {
            Document.Dispose();
            throw;
        }
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
    /// Gets the root element of <see cref="Document"/>. Use <see cref="Document"/> to access the complete HTML document.
    /// </summary>
    [Obsolete("Use Document.DocumentElement instead.")]
    public IElement Element { get; }

    /// <summary>
    /// Gets the parsed HTML document owned by this context.
    /// </summary>
    public IHtmlDocument Document { get; }

    /// <summary>
    /// Gets the selected result elements.
    /// </summary>
    public IHtmlCollection<IElement> ResultElements { get; }

    /// <summary>
    /// Gets the first selected element, or <see langword="null"/> when no element matched.
    /// </summary>
    public IElement? ResultElement => ResultElements.FirstOrDefault();

    /// <summary>
    /// Releases the parsed HTML document. The source HTTP response is not disposed.
    /// </summary>
    /// <remarks>The document and its elements must no longer be used after disposal.</remarks>
    public void Dispose()
    {
        Document.Dispose();
    }
}
