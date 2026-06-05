// ReSharper disable InheritdocInvalidUsage
namespace FclEx.Http;

/// <summary>
/// Base class for handling an XML response without sending the request itself.
/// </summary>
/// <typeparam name="T">The result type produced from the selected XML element.</typeparam>
public abstract class XmlAction<T> : HttpResponseHandler<T>, IXmlAction<T>
{
    /// <inheritdoc />
    public virtual string? XPath => null;

    /// <inheritdoc />
    public virtual OperationResult<string> GetXml(HttpResponse response)
        => DefaultXmlAction.GetXml(this, response);

    /// <inheritdoc />
    public virtual OperationResult<XmlActionContext> CreateContext(HttpResponse response, string xml)
        => DefaultXmlAction.CreateContext(this, response, xml);

    /// <inheritdoc />
    public virtual OperationResult<T> GetResult(XmlActionContext context)
        => DefaultXmlAction.GetResult(this, context);

    /// <inheritdoc />
    public override OperationResult<T> GetResult(HttpResponse response)
        => DefaultXmlAction.GetResult(this, response);
}

/// <summary>
/// Base class for XML response handlers that only need success or failure.
/// </summary>
public abstract class XmlAction : XmlAction<Unit>, IXmlAction
{
    /// <inheritdoc />
    public override OperationResult GetResult(XmlActionContext context) => Operation.Success();
}