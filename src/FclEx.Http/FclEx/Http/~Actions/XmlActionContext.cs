namespace FclEx.Http;

/// <summary>
/// Contains a parsed XML response and the elements selected for an action.
/// </summary>
public readonly struct XmlActionContext
{
    /// <summary>
    /// Initializes an XML action context.
    /// </summary>
    /// <param name="response">The source HTTP response.</param>
    /// <param name="xml">The XML text to parse.</param>
    /// <param name="path">The optional XPath. When <see langword="null"/>, the document root element is selected.</param>
    /// <remarks>Malformed XML may throw during construction.</remarks>
    public XmlActionContext(HttpResponse response, string xml, string? path)
    {
        Response = response;
        Xml = xml;
        Path = path;
        Document = XDocument.Parse(xml);
        ResultElements = path == null
            ? [Document.Root!]
            : Document.XPathSelectElements(path);
    }

    /// <summary>
    /// Gets the source HTTP response.
    /// </summary>
    public HttpResponse Response { get; }

    /// <summary>
    /// Gets the XPath used to select result elements.
    /// </summary>
    public string? Path { get; }

    /// <summary>
    /// Gets the original XML text.
    /// </summary>
    public string Xml { get; }

    /// <summary>
    /// Gets the parsed XML document.
    /// </summary>
    public XDocument Document { get; }

    /// <summary>
    /// Gets the selected result elements.
    /// </summary>
    public IEnumerable<XElement> ResultElements { get; }

    /// <summary>
    /// Gets the first selected element, or <see langword="null"/> when no element matched.
    /// </summary>
    public XElement? ResultElement => ResultElements.FirstOrDefault();
}
