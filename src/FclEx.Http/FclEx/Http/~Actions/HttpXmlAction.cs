namespace FclEx.Http;

public abstract class HttpXmlAction<T> : HttpAction<T>, IXmlAction<T>
{
    public virtual string? XmlResultPath { get; } = null;
    public override OperationResult<T> GetResult(HttpResponse response)
        => DefaultXmlAction.GetResult(this, response);
    public virtual OperationResult<XmlActionContext> CreateContext(HttpResponse response, string xml)
        => DefaultXmlAction.CreateContext(this, response, xml);
    public virtual OperationResult<T> GetResult(XmlActionContext context)
        => DefaultXmlAction.GetResult(this, context);
    public virtual OperationResult<string> GetXml(HttpResponse response)
        => DefaultXmlAction.GetXml(this, response);
}
