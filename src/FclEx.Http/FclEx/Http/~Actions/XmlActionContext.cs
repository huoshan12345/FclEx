namespace FclEx.Http;

#if NET6_0_OR_GREATER
#endif
public readonly struct XmlActionContext
{
    public XmlActionContext(HttpResponse response, string xml, string? path)
    {
        Response = response;
        Xml = xml;
        Path = path;
        Element = XElement.Parse(xml);
        ResultElements = path == null
            ? [Element]
            : Element.XPathSelectElements(path)!;
    }

    public HttpResponse Response { get; }
    public string? Path { get; }
    public string Xml { get; }
    public XElement Element { get; }
    public IEnumerable<XElement> ResultElements { get; }
    public XElement? ResultElement => ResultElements.FirstOrDefault();
}