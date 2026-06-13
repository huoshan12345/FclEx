namespace FclEx.Http;

/// <summary>
/// Describes one file download request.
/// </summary>
public class DownloadOptions
{
    /// <summary>
    /// The request URI to download from.
    /// </summary>
    public required Uri Uri { get; set; }

    /// <summary>
    /// The HTTP method used for the download request.
    /// </summary>
    public HttpMethod Method { get; set; } = HttpMethod.Get;

    /// <summary>
    /// Optional request body content. It is disposed by the download helper after the send completes.
    /// </summary>
    public HttpContent? Content { get; set; }

    /// <summary>
    /// Optional buffer size used while reading response content.
    /// </summary>
    public int? BufferSize { get; set; }

    /// <summary>
    /// Optional timeout for receiving response headers.
    /// </summary>
    public TimeSpan? ReadHeadersTimeout { get; set; }

    /// <summary>
    /// Optional timeout for buffered response-content reading.
    /// </summary>
    public TimeSpan? ReadBufferTimeout { get; set; }

    /// <summary>
    /// Optional total timeout for the entire download operation, including sending the request and reading the response content.
    /// </summary>
    public TimeSpan? TotalTimeout { get; set; }

    /// <summary>
    /// Cancellation token passed to the send and content-reading operations.
    /// </summary>
    public CancellationToken CancellationToken { get; set; } = default;

    /// <summary>
    /// Optional file base name to use in the returned <see cref="HttpFileDownloadInfo"/>.
    /// </summary>
    /// <remarks>When non-null, this value overrides the name derived from response headers or URI.</remarks>
    public string? FileBaseName { get; set; }

    /// <summary>
    /// Optional file extension to use in the returned <see cref="HttpFileDownloadInfo"/>.
    /// </summary>
    /// <remarks>When non-null, this value overrides the extension derived from response headers, MIME type, or URI.</remarks>
    public string? FileExtension { get; set; }
}
