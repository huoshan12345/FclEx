namespace FclEx.Http;

public static class DefaultHtmlAction
{
    public static OperationResult<T> GetResult<T>(IHtmlAction<T> action, HttpResponse response)
    {
        return action.GetHtml(response)
            .Then(m => action.CreateContext(response, m))
            .Then(action.GetResult);
    }

    public static OperationResult<string> GetHtml<T>(IHtmlAction<T> action, HttpResponse response)
    {
        var str = response.ResponseString;
        return str switch
        {
            _ when str.IsNullOrEmpty() => Operation.Error<string>("The response string is empty"),
            _ when str.IsPossibleHtml() => Operation.Success(response.ResponseString),
            _ => Operation.Error<string>("The response string is not a valid html: " + str.Truncate(256))
        };
    }

    public static OperationResult<HtmlActionContext> CreateContext<T>(IHtmlAction<T> action, HttpResponse response, string html)
    {
        var context = new HtmlActionContext(response, html, action.HtmlResultPath);
        if (context.ResultElements.IsNotEmpty())
            return context;

        const string msg = "The result object does not exist in html";
        var error = action.HtmlResultPath == null ? msg : msg + " at " + action.HtmlResultPath;
        error = error + ": " + context.Html.Truncate(256);
        return error;
    }
}