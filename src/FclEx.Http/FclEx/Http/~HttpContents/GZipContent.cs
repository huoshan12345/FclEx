namespace FclEx.Http;

/// <summary>
/// <see cref="HttpContent"/> wrapper that sends the inner content with GZip compression and a <c>gzip</c> Content-Encoding header.
/// </summary>
public class GZipContent : CompressedContent
{
    /// <summary>
    /// Creates a GZip-compressed content wrapper.
    /// </summary>
    /// <param name="content">Inner content to compress while serializing.</param>
    /// <param name="compressionLevel">Compression level used by <see cref="GZipStream"/>.</param>
    /// <param name="timeout">Optional timeout while copying the inner content stream.</param>
    /// <param name="bufferSize">Optional copy buffer size.</param>
    /// <param name="token">Cancellation token used while reading and copying the inner content.</param>
    public GZipContent(HttpContent content, CompressionLevel compressionLevel,
        TimeSpan? timeout = null, int? bufferSize = null, CancellationToken token = default)
        : base(content, "gzip", compressionLevel, timeout, bufferSize, token)
    {
    }

    protected override Stream CreateCompressedStream(Stream stream)
    {
        return new GZipStream(stream, CompressionLevel, true);
    }
}
