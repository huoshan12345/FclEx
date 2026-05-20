namespace FclEx.Http;

/// <summary>
/// Base class for handling an HTML response without sending the request itself.
/// </summary>
/// <typeparam name="T">The result type produced from the selected HTML element.</typeparam>
public abstract class HtmlAction<T> : HttpResponseHandler<T>, IHtmlAction<T>
{
    /// <inheritdoc />
    public virtual string? HtmlResultPath => null;

    /// <inheritdoc />
    public virtual OperationResult<string> GetHtml(HttpResponse response)
        => DefaultHtmlAction.GetHtml(this, response);

    /// <inheritdoc />
    public virtual OperationResult<HtmlActionContext> CreateContext(HttpResponse response, string html)
        => DefaultHtmlAction.CreateContext(this, response, html);

    /// <inheritdoc />
    public abstract OperationResult<T> GetResult(HtmlActionContext context);

    /// <inheritdoc />
    public override OperationResult<T> GetResult(HttpResponse response)
        => DefaultHtmlAction.GetResult(this, response);
}

/// <summary>
/// Base class for HTML response handlers that only need success or failure.
/// </summary>
public abstract class HtmlAction : HtmlAction<Unit>
{
    /// <inheritdoc />
    public override OperationResult GetResult(HtmlActionContext context) => Operation.Success();
}
