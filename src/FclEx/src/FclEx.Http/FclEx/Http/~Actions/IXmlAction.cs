#if NET6_0_OR_GREATER
namespace FclEx.Http;

public interface IXmlAction<T> : IHttpResponseHandler<T>
{
    string? XmlResultPath { get; }

    OperateResult<T> IHttpResponseHandler<T>.GetResult(HttpResponse res)
    {
        var (successful, str, ex, _) = GetXml(res);
        if (!successful)
            return ex!;

        var context = new XmlActionContext(res, str!, XmlResultPath);

        if (IsFailed(context))
            return HandleFailed(context);

        return GetResult(context);
    }

    bool IsFailed(XmlActionContext context) => !context.ResultElements.Any();

    OperateResult<T> HandleFailed(XmlActionContext context)
    {
        const string msg = "The result object does not exist in xml";
        var error = XmlResultPath == null ? msg : msg + " at " + XmlResultPath;
        error = error + ": " + context.Xml.Truncate(256);
        return error;
    }

    OperateResult<string> GetXml(HttpResponse response)
    {
        var str = response.ResponseString;
        return str.IsPossibleXml()
            ? Operate.CreateSuccess(response.ResponseString)
            : Operate.CreateError<string>("The res string is not a valid xml: " + str.Truncate(256));
    }

    OperateResult<T> GetResult(XmlActionContext context) => context.ResultElement!.ToObject<T>()!;
}

public interface IXmlAction : IXmlAction<Unit>
{
    OperateResult IXmlAction<Unit>.GetResult(XmlActionContext context) => Operate.Success;
}
#endif