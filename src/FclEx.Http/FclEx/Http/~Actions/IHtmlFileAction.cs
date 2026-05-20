namespace FclEx.Http;

/// <summary>
/// Represents an HTML action whose response body is loaded from a local file.
/// </summary>
/// <typeparam name="T">The result type produced from the selected HTML element.</typeparam>
public interface IHtmlFileAction<T> : IHttpAction<T>, IHtmlAction<T>
{
    /// <summary>
    /// Gets the local file path to read.
    /// </summary>
    string FilePath { get; }

#if NET6_0_OR_GREATER
    /// <inheritdoc />
    Task<HttpResponse> IHttpAction<T>.GetResponseAsync(HttpRequest request, CancellationToken token)
        => DefaultHtmlFileAction.GetResponseAsync(this, request, token);

    /// <inheritdoc />
    IHttpService IHttpAction<T>.HttpService => HttpClientService.Default;

    /// <inheritdoc />
    Uri IHttpAction<T>.Uri => DefaultHtmlFileAction.GetUri(this);

    /// <inheritdoc />
    HttpMethod IHttpAction<T>.Method => HttpMethod.Get;

    /// <inheritdoc />
    string? IHtmlAction<T>.HtmlResultPath => null;
#endif
}
