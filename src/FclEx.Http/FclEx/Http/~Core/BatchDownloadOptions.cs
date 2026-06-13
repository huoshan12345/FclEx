namespace FclEx.Http;

/// <summary>
/// Describes shared options for downloading multiple files.
/// </summary>
public class BatchDownloadOptions
{
    /// <summary>
    /// Optional base address used to resolve relative download URIs.
    /// </summary>
    public Uri? BaseAddress { get; set; }

    /// <summary>
    /// The HTTP method used for each download request.
    /// </summary>
    public HttpMethod Method { get; set; } = HttpMethod.Get;

    /// <summary>
    /// Optional request body content shared by the batch operation.
    /// </summary>
    /// <remarks>When multiple requests are created, the content is buffered once and cloned for each request.</remarks>
    public HttpContent? Content { get; set; }

    /// <summary>
    /// Optional timeout for receiving response headers.
    /// </summary>
    public TimeSpan? ReadHeadersTimeout { get; set; }

    /// <summary>
    /// Optional buffer size used while reading response content.
    /// </summary>
    public int? BufferSize { get; set; }

    /// <summary>
    /// Optional timeout for buffered response-content reading.
    /// </summary>
    public TimeSpan? ReadBufferTimeout { get; set; }

    /// <summary>
    /// Whether downloads should run concurrently.
    /// </summary>
    public bool ExecuteInParallel { get; set; } = true;

    /// <summary>
    /// Optional maximum concurrency when <see cref="ExecuteInParallel"/> is enabled.
    /// </summary>
    public int? Concurrency { get; set; }

    /// <summary>
    /// Cancellation token passed to each download request.
    /// </summary>
    public CancellationToken CancellationToken { get; set; }
}
