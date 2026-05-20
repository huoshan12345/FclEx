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

    OperationResult<HtmlActionContext> CreateContext(HttpResponse response, string html)
#if NET6_0_OR_GREATER
        => DefaultHtmlAction.CreateContext(this, response, html);
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