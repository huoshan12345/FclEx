namespace FclEx.Http;

public interface IHtmlFileAction<T> : IHttpAction<T>, IHtmlAction<T>
{
    string FilePath { get; }

#if NET6_0_OR_GREATER
    Task<HttpResponse> IHttpAction<T>.GetResponseAsync(HttpRequest request, CancellationToken token)
        => DefaultHtmlFileAction.GetResponseAsync(this, request, token);

    IHttpService IHttpAction<T>.HttpService => HttpClientService.Default;
    Uri IHttpAction<T>.Uri => DefaultHtmlFileAction.GetUri(this);
    HttpMethod IHttpAction<T>.Method => HttpMethod.Get;
    string? IHtmlAction<T>.HtmlResultPath => null;
#endif
}