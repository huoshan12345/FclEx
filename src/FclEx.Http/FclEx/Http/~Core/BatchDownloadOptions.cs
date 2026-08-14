namespace FclEx.Http;

/// <summary>
/// Describes shared options for downloading multiple files.
/// </summary>
public class BatchDownloadOptions
{
    /// <summary>
    /// Default upper bound for simultaneously active downloads.
    /// </summary>
    public const int DefaultMaxDegreeOfParallelism = 8;

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
    /// Optional total timeout for the entire download operation, including sending the request and reading the response content.
    /// </summary>
    public TimeSpan? TotalTimeout { get; set; }

    /// <summary>
    /// Maximum number of downloads that may be active at the same time.
    /// </summary>
    /// <remarks>Set this to 1 for sequential execution.</remarks>
    public int MaxDegreeOfParallelism { get; set; } = DefaultMaxDegreeOfParallelism;

    /// <summary>
    /// Cancellation token passed to each download request.
    /// </summary>
    public CancellationToken CancellationToken { get; set; }
}
