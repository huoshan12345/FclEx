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
        => DefaultHtmlAction.GetResult(this, response);
#endif

    OperationResult<string> GetHtml(HttpResponse response)
#if NET6_0_OR_GREATER
        => DefaultHtmlAction.GetHtml(this, response);
#else
    ;
#endif

    OperationResult<HtmlActionContext> CreateContext(HttpResponse response, string json)
#if NET6_0_OR_GREATER
        => DefaultHtmlAction.CreateContext(this, response, json);
#else
    ;
#endif
}

public interface IHtmlAction : IHtmlAction<Unit>
{
#if NET6_0_OR_GREATER
    OperationResult IHtmlAction<Unit>.GetResult(HtmlActionContext context) => Operation.Success();
#endif
}

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

    public static OperationResult<HtmlActionContext> CreateContext<T>(IHtmlAction<T> action, HttpResponse response, string json)
    {
        var context = new HtmlActionContext(response, json, action.HtmlResultPath);
        if (context.ResultElements.IsNotEmpty())
            return context;

        const string msg = "The result object does not exist in html";
        var error = action.HtmlResultPath == null ? msg : msg + " at " + action.HtmlResultPath;
        error = error + ": " + context.Html.Truncate(256);
        return error;
    }
}

public abstract class HtmlAction<T> : HttpResponseHandler<T>, IHtmlAction<T>
{
    public virtual string? HtmlResultPath => null;
    public virtual OperationResult<string> GetHtml(HttpResponse response)
        => DefaultHtmlAction.GetHtml(this, response);
    public virtual OperationResult<HtmlActionContext> CreateContext(HttpResponse response, string json)
        => DefaultHtmlAction.CreateContext(this, response, json);
    public abstract OperationResult<T> GetResult(HtmlActionContext context);
    public override OperationResult<T> GetResult(HttpResponse response)
        => DefaultHtmlAction.GetResult(this, response);
}

public abstract class HtmlAction : HtmlAction<Unit>
{
    public override OperationResult GetResult(HtmlActionContext context) => Operation.Success();
}