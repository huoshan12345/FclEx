using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AngleSharp.Dom;
using FclEx.Http.Core;
using FclEx.Utils;

namespace FclEx.Actions
{
    public interface IHtmlAction<T> : IHttpResHandler<T>
    {
        string? HtmlResultPath { get; }

        OperateResult<T> IHttpResHandler<T>.GetResult(HttpRes res)
        {
            var (successful, _, str, ex) = GetHtmlString(res);
            if (!successful)
                return ex!;

            var context = new HtmlActionContext(res, str!, HtmlResultPath);
            
            if (IsFailed(context))
                return HandleFailed(context);

            return GetResult(context);
        }

        OperateResult<string> GetHtmlString(HttpRes response)
        {
            var str = response.ResponseString;
            return str switch
            {
                _ when str.IsNullOrEmpty() => OperateResult.CreateError("The res string is empty"),
                _ when str.IsPossibleHtml() => OperateResult.CreateSuccess(response.ResponseString),
                _ => OperateResult.CreateError("The res string is not a valid html: " + str.TruncateSafely(256))
            };
        }

        bool IsFailed(HtmlActionContext context) => context.ResultElements.Any();

        OperateResult<T> HandleFailed(HtmlActionContext context)
        {
            const string msg = "The result object does not exist in html";
            var error = HtmlResultPath == null ? msg : msg + " at " + HtmlResultPath;
            error = error + ": " + context.Html.TruncateSafely(256);
            return error;
        }

        OperateResult<T> GetResult(HtmlActionContext context);
    }

    public interface IHtmlAction : IHtmlAction<Unit>
    {
        OperateResult<Unit> IHtmlAction<Unit>.GetResult(HtmlActionContext context) => OperateResult.Success;
    }

    public readonly struct HtmlActionContext
    {
        public HtmlActionContext(HttpRes httpRes, string html, string? path)
        {
            HttpRes = httpRes;
            Html = html;
            Path = path;
            Element = HtmlHelper.Parse(html).DocumentElement;
            ResultElements = path == null
                ? Element.Yield().ToCollection()
                : Element.QuerySelectorAll(path)!;
        }

        public HttpRes HttpRes { get; }
        public string? Path { get; }
        public string Html { get; }
        public IElement Element { get; }
        public IHtmlCollection<IElement> ResultElements { get; }
        public IElement? ResultElement => ResultElements.FirstOrDefault();
    }
}
