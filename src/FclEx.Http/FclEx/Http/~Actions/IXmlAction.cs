namespace FclEx.Http;

public interface IXmlAction<T> : IHttpResponseHandler<T>
{
    string? XmlResultPath
#if NET6_0_OR_GREATER
        => null;
#else
    { get; }
#endif

#if NET6_0_OR_GREATER
    OperationResult<T> IHttpResponseHandler<T>.GetResult(HttpResponse response)
        => DefaultXmlAction.GetResult(this, response);
#endif

    OperationResult<string> GetXml(HttpResponse response)
#if NET6_0_OR_GREATER
        => DefaultXmlAction.GetXml(this, response);
#else
    ;
#endif

    OperationResult<XmlActionContext> CreateContext(HttpResponse response, string xml)
#if NET6_0_OR_GREATER
        => DefaultXmlAction.CreateContext(this, response, xml);
#else
    ;
#endif

    OperationResult<T> GetResult(XmlActionContext context)
#if NET6_0_OR_GREATER
        => DefaultXmlAction.GetResult(this, context);
#else
    ;
#endif
}

public interface IXmlAction : IXmlAction<Unit>
{
#if NET6_0_OR_GREATER
    OperationResult IXmlAction<Unit>.GetResult(XmlActionContext context) => Operation.Success();
#endif
}

public static class DefaultXmlAction
{
    public static OperationResult<T> GetResult<T>(IXmlAction<T> action, HttpResponse response)
    {
        return action.GetXml(response)
            .Then(m => action.CreateContext(response, m))
            .Then(action.GetResult);
    }

    public static OperationResult<string> GetXml<T>(IXmlAction<T> action, HttpResponse response)
    {
        var str = response.ResponseString;
        return str.IsPossibleXml()
            ? Operation.Success(response.ResponseString)
            : Operation.Error<string>("The response string is not a valid xml: " + str.Truncate(256));
    }

    public static OperationResult<XmlActionContext> CreateContext<T>(IXmlAction<T> action, HttpResponse response, string json)
    {
        var context = new XmlActionContext(response, json, action.XmlResultPath);
        if (context.ResultElements.IsNotEmpty())
            return context;
        const string msg = "The result object does not exist in xml";
        var error = action.XmlResultPath == null ? msg : msg + " at " + action.XmlResultPath;
        error = error + ": " + context.Xml.Truncate(256);
        return error;
    }

    public static OperationResult<T> GetResult<T>(IXmlAction<T> action, XmlActionContext context)
    {
        return context.ResultElement is { } element
            ? element.ToObject<T>()!
            : nameof(context.ResultElement) + " is null";
    }
}

public abstract class XmlAction<T> : HttpResponseHandler<T>, IXmlAction<T>
{
    public virtual string? XmlResultPath => null;
    public virtual OperationResult<string> GetXml(HttpResponse response) => DefaultXmlAction.GetXml(this, response);
    public virtual OperationResult<XmlActionContext> CreateContext(HttpResponse response, string xml)
        => DefaultXmlAction.CreateContext(this, response, xml);
    public virtual OperationResult<T> GetResult(XmlActionContext context) => DefaultXmlAction.GetResult(this, context);
}

public abstract class XmlAction : XmlAction<Unit>, IXmlAction
{
    public override OperationResult GetResult(XmlActionContext context) => Operation.Success();
}