using System.Diagnostics.CodeAnalysis;
using AngleSharp.Dom;
using FclEx.Http.Core;
using FclEx.Utils;

namespace FclEx.Actions
{
    public interface IHtmlAction<T>
    {
        string? HtmlResultPath { get; }

        OperateResult<T> GetResult(HttpRes response)
        {
            var (successful, _, str, ex) = GetHtmlString(response);
            if (!successful)
                return ex!;

            var element = HtmlHelper.Parse(str!).DocumentElement;
            var resultElement = HtmlResultPath == null
                ? element
                : element.QuerySelector(HtmlResultPath);

            var (hasError, error) = GetHtmlError(response, element, resultElement);
            if (hasError)
                return error;

            return GetResult(response, resultElement);
        }

        OperateResult<string> GetHtmlString(HttpRes response)
        {
            var str = response.ResponseString;
            return str.IsPossibleHtml()
                ? OperateResult.CreateSuccess(response.ResponseString)
                : OperateResult.CreateError<string>("The response string is not a valid html: " + str.TruncateSafely(256));
        }

        StringError GetHtmlError(HttpRes response, IElement element, IElement? resultElement)
        {
            if (resultElement == null)
            {
                const string msg = "The result object does not exist in html";
                var error = HtmlResultPath == null ? msg : msg + " at " + HtmlResultPath;
                error = error + ": " + response.ResponseString.TruncateSafely(256);
                return (true, error);
            }
            return (false, "");
        }

        OperateResult<T> GetResult(HttpRes response, IElement element);
    }

    public interface IHtmlAction : IHtmlAction<Unit>
    {
        OperateResult<Unit> IHtmlAction<Unit>.GetResult(HttpRes response, IElement token) => OperateResult.Success;
    }
}
