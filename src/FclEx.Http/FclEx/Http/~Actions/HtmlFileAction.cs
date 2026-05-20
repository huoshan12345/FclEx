namespace FclEx.Http;

public abstract class HtmlFileAction<T> : HttpAction<T>, IHtmlFileAction<T>
{
    public abstract string FilePath { get; }
    public virtual string? HtmlResultPath { get; } = null;
    public override IHttpService HttpService { get; } = HttpClientService.Default;
    public override Uri Uri => field ??= DefaultHtmlFileAction.GetUri(this);
    public override HttpMethod Method { get; } = HttpMethod.Get;

    public override Task<HttpResponse> GetResponseAsync(HttpRequest request, CancellationToken token = default)
        => DefaultHtmlFileAction.GetResponseAsync(this, request, token);
    public override OperationResult<T> GetResult(HttpResponse response)
        => DefaultHtmlAction.GetResult(this, response);
    public virtual OperationResult<string> GetHtml(HttpResponse response)
        => DefaultHtmlAction.GetHtml(this, response);
    public virtual OperationResult<HtmlActionContext> CreateContext(HttpResponse response, string html)
        => DefaultHtmlAction.CreateContext(this, response, html);
    public abstract OperationResult<T> GetResult(HtmlActionContext context);
}