#if NET6_0_OR_GREATER
namespace FclEx.Http;

public interface IHtmlAction<T> : IHttpResponseHandler<T>
{
    string? HtmlResultPath { get; }

    OperateResult<T> IHttpResponseHandler<T>.GetResult(HttpResponse res)
    {
        var (successful, str, ex, _) = GetHtml(res);
        if (!successful)
            return ex!;

        var context = new HtmlActionContext(res, str!, HtmlResultPath);

        return IsFailed(context)
            ? HandleFailed(context)
            : GetResult(context);
    }

    OperateResult<string> GetHtml(HttpResponse res)
    {
        var str = res.ResponseString;
        return str switch
        {
            _ when str.IsNullOrEmpty() => Operate.CreateError<string>("The res string is empty"),
            _ when str.IsPossibleHtml() => Operate.CreateSuccess(res.ResponseString),
            _ => Operate.CreateError<string>("The res string is not a valid html: " + str.Truncate(256))
        };
    }

    bool IsFailed(HtmlActionContext context) => context.ResultElements.Any();

    OperateResult<T> HandleFailed(HtmlActionContext context)
    {
        const string msg = "The result object does not exist in html";
        var error = HtmlResultPath == null ? msg : msg + " at " + HtmlResultPath;
        error = error + ": " + context.Html.Truncate(256);
        return error;
    }

    OperateResult<T> GetResult(HtmlActionContext context);
}

public interface IHtmlAction : IHtmlAction<Unit>
{
    OperateResult IHtmlAction<Unit>.GetResult(HtmlActionContext context) => Operate.Success;
}
#endif