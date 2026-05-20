namespace FclEx.Http;

public readonly struct XmlActionContext
{
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

    public HttpResponse Response { get; }
    public string? Path { get; }
    public string Xml { get; }
    public XDocument Document { get; }
    public IEnumerable<XElement> ResultElements { get; }
    public XElement? ResultElement => ResultElements.FirstOrDefault();
}