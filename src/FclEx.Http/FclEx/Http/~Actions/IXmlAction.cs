#if NET6_0_OR_GREATER
namespace FclEx.Http;

public interface IXmlAction<T> : IHttpResponseHandler<T>
{
    string? XmlResultPath { get; }

    OperationResult<T> IHttpResponseHandler<T>.GetResult(HttpResponse response)
    {
        var (successful, str, ex, _) = GetXml(response);
        if (!successful)
            return ex!;

        var context = new XmlActionContext(response, str!, XmlResultPath);

        if (IsFailed(context))
            return HandleFailed(context);

        return GetResult(context);
    }

    bool IsFailed(XmlActionContext context) => !context.ResultElements.Any();

    OperationResult<T> HandleFailed(XmlActionContext context)
    {
        const string msg = "The result object does not exist in xml";
        var error = XmlResultPath == null ? msg : msg + " at " + XmlResultPath;
        error = error + ": " + context.Xml.Truncate(256);
        return error;
    }

    OperationResult<string> GetXml(HttpResponse response)
    {
        var str = response.ResponseString;
        return str.IsPossibleXml()
            ? Operation.CreateSuccess(response.ResponseString)
            : Operation.Error<string>("The response string is not a valid xml: " + str.Truncate(256));
    }

    OperationResult<T> GetResult(XmlActionContext context) => context.ResultElement!.ToObject<T>()!;
}

public interface IXmlAction : IXmlAction<Unit>
{
    OperationResult IXmlAction<Unit>.GetResult(XmlActionContext context) => Operation.Success();
}
#endif