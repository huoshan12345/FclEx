using System.Diagnostics.CodeAnalysis;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using FclEx.Extensions;
using FclEx.Http.Core;
using FclEx.Utils;
using JetBrains.Annotations;

namespace FclEx.Actions
{
    public interface IHtmlAction<TObject, TResult>
    {
        sealed OperateResult<TResult> GetResultBody(HttpRes response)
        {
            var (successful, _, str, ex) = GetHtmlString(response);
            if (!successful)
                return ex!;

            if (!str.IsPossibleHtml())
                return HandleNonHtmlResult(response);

            var element = HtmlHelper.Parse(str).DocumentElement;
            if (TryGetHtmlError(response, element, out var htmlError))
                return htmlError;

            var (successfulOfHtml, _, obj, exOfHtml) = HandleHtml(response, element);
            if (!successfulOfHtml)
                return exOfHtml!;

            if (TryGetHtmlObjectError(response, element, obj!, out var htmlObjectError))
                return htmlObjectError;

            return HandleResult(response, element, obj!);
        }

        OperateResult<TResult> GetResult(HttpRes response) => GetResultBody(response);

        OperateResult<string> GetHtmlString(HttpRes response)
        {
            return OperateResult.CreateSuccess(response.ResponseString);
        }

        OperateResult<TResult> HandleNonHtmlResult(HttpRes response)
        {
            return "The response string is not a valid html: " + response.ResponseString.TruncateSafely(256);
        }

        bool TryGetHtmlError(HttpRes response, IElement element, [NotNullWhen(true)] out string? error)
        {
            error = default;
            return false;
        }

        OperateResult<TObject> HandleHtml(HttpRes response, IElement element);

        bool TryGetHtmlObjectError(HttpRes response, IElement element, TObject obj, [NotNullWhen(true)] out string? error)
        {
            error = default;
            return false;
        }

        OperateResult<TResult> HandleResult(HttpRes response, IElement element, TObject obj);
    }

    public interface IHtmlAction<TResult> : IHtmlAction<TResult, TResult>
    {
        OperateResult<TResult> IHtmlAction<TResult, TResult>.HandleResult(HttpRes response, IElement element, TResult obj) => (obj, response.ExcuteTime);
    }

    public interface IHtmlAction : IHtmlAction<Unit>
    {
        OperateResult<Unit> IHtmlAction<Unit, Unit>.HandleHtml(HttpRes response, IElement element) => OperateResult.Success;
        OperateResult<Unit> IHtmlAction<Unit, Unit>.HandleResult(HttpRes response, IElement token, Unit obj) => obj;
    }
}
