// ReSharper disable InheritdocInvalidUsage
namespace FclEx.Http;

/// <summary>
/// Base class for HTML actions that read their response from a local file.
/// </summary>
/// <typeparam name="T">The result type produced from the selected HTML element.</typeparam>
public abstract class HtmlFileAction<T> : HttpAction<T>, IHtmlFileAction<T>
{
    /// <inheritdoc />
    public abstract string FilePath { get; }

    /// <inheritdoc />
    public virtual string? HtmlSelector { get; } = null;

    /// <inheritdoc />
    public override IHttpService HttpService { get; } = HttpClientService.Default;

    /// <inheritdoc />
    public override Uri Uri => field ??= DefaultHtmlFileAction.GetUri(this);

    /// <inheritdoc />
    public override HttpMethod Method { get; } = HttpMethod.Get;

    /// <inheritdoc />
    public override Task<HttpResponse> GetResponseAsync(HttpRequest request, CancellationToken token = default)
        => DefaultHtmlFileAction.GetResponseAsync(this, request, token);

    /// <inheritdoc />
    public override OperationResult<T> GetResult(HttpResponse response)
        => DefaultHtmlAction.GetResult(this, response);

    /// <inheritdoc />
    public virtual OperationResult<string> GetHtml(HttpResponse response)
        => DefaultHtmlAction.GetHtml(this, response);

    /// <inheritdoc />
    public virtual OperationResult<HtmlActionContext> CreateContext(HttpResponse response, string html)
        => DefaultHtmlAction.CreateContext(this, response, html);

    /// <inheritdoc />
    public abstract OperationResult<T> GetResult(HtmlActionContext context);
}
