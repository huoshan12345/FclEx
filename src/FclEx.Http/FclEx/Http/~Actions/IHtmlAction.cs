#if NET6_0_OR_GREATER
namespace FclEx.Http;

public interface IHtmlAction<T> : IHttpResponseHandler<T>
{
    string? HtmlResultPath { get; }

    OperationResult<T> IHttpResponseHandler<T>.GetResult(HttpResponse response)
    {
        var (successful, str, ex, _) = GetHtml(response);
        if (!successful)
            return ex!;

        var context = new HtmlActionContext(response, str!, HtmlResultPath);

        return IsFailed(context)
            ? HandleFailed(context)
            : GetResult(context);
    }

    OperationResult<string> GetHtml(HttpResponse response)
    {
        var str = response.ResponseString;
        return str switch
        {
            _ when str.IsNullOrEmpty() => Operation.Error<string>("The response string is empty"),
            _ when str.IsPossibleHtml() => Operation.Success(response.ResponseString),
            _ => Operation.Error<string>("The response string is not a valid html: " + str.Truncate(256))
        };
    }

    bool IsFailed(HtmlActionContext context) => context.ResultElements.IsNullOrEmpty();

    OperationResult<T> HandleFailed(HtmlActionContext context)
    {
        const string msg = "The result object does not exist in html";
        var error = HtmlResultPath == null ? msg : msg + " at " + HtmlResultPath;
        error = error + ": " + context.Html.Truncate(256);
        return error;
    }

    OperationResult<T> GetResult(HtmlActionContext context);
}

public interface IHtmlAction : IHtmlAction<Unit>
{
    OperationResult IHtmlAction<Unit>.GetResult(HtmlActionContext context) => Operation.Success();
}
#endif