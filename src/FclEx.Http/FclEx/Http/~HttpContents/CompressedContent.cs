namespace FclEx.Http;

/// <summary>
/// Base <see cref="HttpContent"/> wrapper that compresses an inner content stream while it is being serialized.
/// The wrapper copies most headers from the inner content, replaces Content-Encoding, and cannot precompute Content-Length.
/// </summary>
public abstract class CompressedContent : HttpContent
{
    /// <summary>
    /// Default buffer size used while copying the inner content stream into the compression stream.
    /// </summary>
    public const int DefaultBufferSize = 256 * 1024;

    /// <summary>
    /// Inner content that will be compressed during serialization.
    /// It is disposed when this wrapper is disposed.
    /// </summary>
    public HttpContent Content { get; }

    /// <summary>
    /// Buffer size used when copying the inner content stream to the compression stream.
    /// </summary>
    public int BufferSize { get; }

    /// <summary>
    /// Optional timeout applied while copying the inner content stream.
    /// </summary>
    public TimeSpan? Timeout { get; }

    /// <summary>
    /// Cancellation token used when reading and copying the inner content stream.
    /// </summary>
    public CancellationToken Token { get; }

    /// <summary>
    /// Compression level passed to the concrete compression stream.
    /// </summary>
    public CompressionLevel CompressionLevel { get; }

    /// <summary>
    /// Creates a compressed content wrapper and sets the outgoing Content-Encoding header.
    /// Content-Length and any existing Content-Encoding header from the inner content are intentionally not copied.
    /// </summary>
    protected CompressedContent(HttpContent content, string encoding, CompressionLevel compressionLevel,
        TimeSpan? timeout = null, int? bufferSize = null, CancellationToken token = default)
    {
        Content = content;
        Timeout = timeout;
        Token = token;
        BufferSize = bufferSize ?? DefaultBufferSize;
        CompressionLevel = compressionLevel;
        content.Headers.CopyTo(Headers, HttpHeaderNames.ContentLength, HttpHeaderNames.ContentEncoding);
        Headers.Add(HttpHeaderNames.ContentEncoding, encoding);
    }

    /// <summary>
    /// Creates the compression stream that writes compressed bytes to the outgoing HTTP stream.
    /// </summary>
    protected abstract Stream CreateCompressedStream(Stream stream);

    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
#if NET6_0_OR_GREATER
        await
#endif
        using var contentStream = await Content.ReadAsStreamAsync(Token);
#if NET6_0_OR_GREATER
        await
#endif
        using var compressedStream = CreateCompressedStream(stream);
        await contentStream.CopyToAsync(compressedStream, BufferSize, Timeout, Token);
    }

    protected override bool TryComputeLength(out long length)
    {
        length = 0;
        return false;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Content.Dispose();
        }
        base.Dispose(disposing);
    }
}
