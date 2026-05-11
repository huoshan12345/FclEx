namespace FclEx.Http;

public interface IHtmlAction<T> : IHttpResponseHandler<T>
{
    string? HtmlResultPath
#if NET6_0_OR_GREATER
        => null;
#else
    { get; }
#endif

    OperationResult<T> GetResult(HtmlActionContext context);

#if NET6_0_OR_GREATER
    OperationResult<T> IHttpResponseHandler<T>.GetResult(HttpResponse response)
    {
        return GetHtml(response)
            .Then(m => CreateContext(response, m))
            .Then(GetResult);
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

    OperationResult<HtmlActionContext> CreateContext(HttpResponse response, string json)
    {
        var context = new HtmlActionContext(response, json, HtmlResultPath);
        if (context.ResultElements.IsNotEmpty())
            return context;

        const string msg = "The result object does not exist in html";
        var error = HtmlResultPath == null ? msg : msg + " at " + HtmlResultPath;
        error = error + ": " + context.Html.Truncate(256);
        return error;
    }
#endif
}

public interface IHtmlAction : IHtmlAction<Unit>
{
#if NET6_0_OR_GREATER
    OperationResult IHtmlAction<Unit>.GetResult(HtmlActionContext context) => Operation.Success();
#endif
}
