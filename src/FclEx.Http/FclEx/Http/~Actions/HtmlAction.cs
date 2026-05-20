namespace FclEx.Http;

public abstract class HtmlAction<T> : HttpResponseHandler<T>, IHtmlAction<T>
{
    public virtual string? HtmlResultPath => null;
    public virtual OperationResult<string> GetHtml(HttpResponse response)
        => DefaultHtmlAction.GetHtml(this, response);
    public virtual OperationResult<HtmlActionContext> CreateContext(HttpResponse response, string html)
        => DefaultHtmlAction.CreateContext(this, response, html);
    public abstract OperationResult<T> GetResult(HtmlActionContext context);
    public override OperationResult<T> GetResult(HttpResponse response)
        => DefaultHtmlAction.GetResult(this, response);
}

public abstract class HtmlAction : HtmlAction<Unit>
{
    public override OperationResult GetResult(HtmlActionContext context) => Operation.Success();
}