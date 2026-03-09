#if NET6_0_OR_GREATER
namespace FclEx.Http;

public interface IXmlAction<T> : IHttpResponseHandler<T>
{
    string? XmlResultPath { get; }

    OperationResult<T> IHttpResponseHandler<T>.GetResult(HttpResponse response)
    {
        return GetXml(response)
            .Then(m => CreateContext(response, m))
            .Then(GetResult);
    }

    OperationResult<string> GetXml(HttpResponse response)
    {
        var str = response.ResponseString;
        return str.IsPossibleXml()
            ? Operation.Success(response.ResponseString)
            : Operation.Error<string>("The response string is not a valid xml: " + str.Truncate(256));
    }

    OperationResult<XmlActionContext> CreateContext(HttpResponse response, string json)
    {
        var context = new XmlActionContext(response, json, XmlResultPath);
        if (context.ResultElements.IsNotEmpty())
            return context;

        const string msg = "The result object does not exist in xml";
        var error = XmlResultPath == null ? msg : msg + " at " + XmlResultPath;
        error = error + ": " + context.Xml.Truncate(256);
        return error;
    }

    OperationResult<T> GetResult(XmlActionContext context)
    {
        return context.ResultElement is { } element
            ? element.ToObject<T>()!
            : nameof(context.ResultElement) + " is null";
    }
}

public interface IXmlAction : IXmlAction<Unit>
{
    OperationResult IXmlAction<Unit>.GetResult(XmlActionContext context) => Operation.Success();
}
#endif