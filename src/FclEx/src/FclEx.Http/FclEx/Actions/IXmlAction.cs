using System.Xml.Linq;
using System.Xml.XPath;
using FclEx;

namespace FclEx.Actions;

public interface IXmlAction<T> : IHttpResHandler<T>
{
    string? XmlResultPath { get; }

    OperateResult<T> IHttpResHandler<T>.GetResult(HttpRes res)
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

    OperateResult<string> GetXml(HttpRes response)
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
    OperateResult<Unit> IXmlAction<Unit>.GetResult(XmlActionContext context) => Operate.Success;
}

public readonly struct XmlActionContext
{
    public XmlActionContext(HttpRes httpRes, string xml, string? path)
    {
        HttpRes = httpRes;
        Xml = xml;
        Path = path;
        Element = XElement.Parse(xml);
        ResultElements = path == null
            ? Element.Yield()
            : Element.XPathSelectElements(path)!;
    }

    public HttpRes HttpRes { get; }
    public string? Path { get; }
    public string Xml { get; }
    public XElement Element { get; }
    public IEnumerable<XElement> ResultElements { get; }
    public XElement? ResultElement => ResultElements.FirstOrDefault();
}