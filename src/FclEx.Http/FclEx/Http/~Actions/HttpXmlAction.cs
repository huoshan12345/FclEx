namespace FclEx.Http;

/// <summary>
/// Base class for HTTP actions whose response body is XML.
/// </summary>
/// <typeparam name="T">The result type produced from the selected XML element.</typeparam>
public abstract class HttpXmlAction<T> : HttpAction<T>, IXmlAction<T>
{
    /// <inheritdoc />
    public virtual string? XmlResultPath { get; } = null;

    /// <inheritdoc />
    public override OperationResult<T> GetResult(HttpResponse response)
        => DefaultXmlAction.GetResult(this, response);

    /// <inheritdoc />
    public virtual OperationResult<XmlActionContext> CreateContext(HttpResponse response, string xml)
        => DefaultXmlAction.CreateContext(this, response, xml);

    /// <inheritdoc />
    public virtual OperationResult<T> GetResult(XmlActionContext context)
        => DefaultXmlAction.GetResult(this, context);

    /// <inheritdoc />
    public virtual OperationResult<string> GetXml(HttpResponse response)
        => DefaultXmlAction.GetXml(this, response);
}
