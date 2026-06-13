#if NET5_0_OR_GREATER
namespace FclEx.Http;

/// <summary>
/// <see cref="HttpContent"/> wrapper that sends the inner content with Brotli compression and a <c>br</c> Content-Encoding header.
/// </summary>
public class BrotliContent : CompressedContent
{
    /// <summary>
    /// Creates a Brotli-compressed content wrapper.
    /// </summary>
    /// <param name="content">Inner content to compress while serializing.</param>
    /// <param name="compressionLevel">Compression level used by <see cref="BrotliStream"/>.</param>
    /// <param name="timeout">Optional timeout while copying the inner content stream.</param>
    /// <param name="bufferSize">Optional copy buffer size.</param>
    /// <param name="token">Cancellation token used while reading and copying the inner content.</param>
    public BrotliContent(HttpContent content, CompressionLevel compressionLevel,
        TimeSpan? timeout = null, int? bufferSize = null, CancellationToken token = default)
        : base(content, "br", compressionLevel, timeout, bufferSize, token)
    {
    }

    protected override Stream CreateCompressedStream(Stream stream)
    {
        return new BrotliStream(stream, CompressionLevel, true);
    }
}
#endif
