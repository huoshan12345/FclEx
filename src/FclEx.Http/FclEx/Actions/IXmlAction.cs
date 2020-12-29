using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using FclEx.Extensions;
using FclEx.Http.Core;
using FclEx.Utils;
using FclEx.Web.Core;
using Newtonsoft.Json.Linq;

namespace FclEx.Actions
{
    public interface IXmlAction<TObject, TResult>
    {
        string? XmlResultPath { get; }

        sealed OperateResult<TResult> GetResult(HttpRes response)
        {
            var (successful, _, str, ex) = GetXmlString(response);
            if (!successful)
                return ex!;

            if (!str.IsPossibleXml())
                return HandleNonXmlResult(response);

            var element = XElement.Parse(str);
            if (!IsSuccessfulXmlElement(response, element))
                return HandleUnsuccessfulXmlElement(response, str, element);

            element = XmlResultPath == null
                ? element
                : element.XPathSelectElement(XmlResultPath);

            if (element == null)
                return HandleXmlPathNonExist(response);

            var (successfulOfXml, _, obj, exOfXml) = HandleXml(response, element);
            if (!successfulOfXml)
                return exOfXml!;

            if (!IsSuccessfulXmlObject(response, obj!))
                return HandleUnsuccessfulXmlObject(response, element, obj!);

            return HandleResult(response, element, obj!);
        }

        OperateResult<string> GetXmlString(HttpRes response)
        {
            return OperateResult.CreateSuccess(response.ResponseString);
        }

        OperateResult<TResult> HandleNonXmlResult(HttpRes response)
        {
            return "The response string is not a valid xml: " + response.ResponseString.TruncateSafely(256);
        }

        bool IsSuccessfulXmlElement(HttpRes response, XElement element) => true;

        OperateResult<TResult> HandleUnsuccessfulXmlElement(HttpRes response, string xml, XElement element)
        {
            return "The xml is unsuccessful: " + xml.TruncateSafely(256);
        }

        OperateResult<TResult> HandleXmlPathNonExist(HttpRes response)
        {
            const string msg = "The result object does not exist in xml";
            var error = XmlResultPath == null ? msg : msg + " at " + XmlResultPath;
            return (error + ": " + response.ResponseString.TruncateSafely(256), response.ExcuteTime);
        }

        OperateResult<TObject> HandleXml(HttpRes response, XElement element) => element.ToObject<TObject>();

        bool IsSuccessfulXmlObject(HttpRes response, TObject obj) => true;

        OperateResult<TResult> HandleUnsuccessfulXmlObject(HttpRes response, XElement element, TObject obj)
        {
            return "The result is unsuccessful: " + element.ToString().TruncateSafely(256);
        }

        OperateResult<TResult> HandleResult(HttpRes response, XElement element, TObject obj);
    }

    public interface IXmlAction<TResult> : IXmlAction<TResult, TResult>
    {
        OperateResult<TResult> IXmlAction<TResult, TResult>.HandleResult(HttpRes response, XElement token, TResult obj) => (obj, response.ExcuteTime);
        OperateResult<TResult> IXmlAction<TResult, TResult>.HandleUnsuccessfulXmlObject(HttpRes response, XElement element, TResult obj) => HandleUnsuccessfulResult(response, element, obj);
        OperateResult<TResult> HandleUnsuccessfulResult(HttpRes response, XElement element, TResult obj) => HandleUnsuccessfulXmlObject(response, element, obj);
        bool IXmlAction<TResult, TResult>.IsSuccessfulXmlObject(HttpRes response, TResult obj) => IsSuccessfulResult(response, obj);
        bool IsSuccessfulResult(HttpRes response, TResult obj) => true;
    }

    public interface IXmlAction : IXmlAction<Unit>
    {
        OperateResult<Unit> IXmlAction<Unit, Unit>.HandleXml(HttpRes response, XElement element) => OperateResult.Success;
        OperateResult<Unit> IXmlAction<Unit, Unit>.HandleResult(HttpRes response, XElement token, Unit obj) => obj;
    }
}
