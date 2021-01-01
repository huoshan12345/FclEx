using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using System.Xml.XPath;
using FclEx.Http.Core;
using FclEx.Utils;

namespace FclEx.Actions
{
    public interface IXmlAction<T>
    {
        string? XmlResultPath { get; }

        OperateResult<T> GetResult(HttpRes response)
        {
            var (successful, _, str, ex) = GetXmlString(response);
            if (!successful)
                return ex!;

            var element = XElement.Parse(str!);
            var resultElement = XmlResultPath == null
                ? element
                : element.XPathSelectElement(XmlResultPath);

            return GetResult(response, str!, element, resultElement);
        }

        StringError GetXElementError(HttpRes response, string xml, XElement element, XElement? resultElement)
        {
            if (resultElement == null)
            {
                const string msg = "The result object does not exist in xml";
                var error = XmlResultPath == null ? msg : msg + " at " + XmlResultPath;
                error = error + ": " + response.ResponseString.TruncateSafely(256);
                return (true, error);
            }
            return (false, "");
        }

        OperateResult<string> GetXmlString(HttpRes response)
        {
            var str = response.ResponseString;
            return str.IsPossibleXml()
                ? OperateResult.CreateSuccess(response.ResponseString)
                : OperateResult.CreateError<string>("The response string is not a valid xml: " + str.TruncateSafely(256));
        }

        OperateResult<T> GetResult(HttpRes response, string xml, XElement element, XElement? resultElement)
        {
            var (hasError, error) = GetXElementError(response, xml, element, resultElement);
            if (hasError)
                return error;
            return resultElement!.ToObject<T>()!;
        }
    }

    public interface IXmlAction : IXmlAction<Unit>
    {
        OperateResult<Unit> IXmlAction<Unit>.GetResult(HttpRes response, string xml,
            XElement element, XElement? resultElement) => OperateResult.Success;
    }
}
