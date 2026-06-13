namespace FclEx.Http;

/// <summary>
/// <see cref="HttpContent"/> wrapper that sends the inner content with deflate compression and a <c>deflate</c> Content-Encoding header.
/// On modern target frameworks it uses zlib-wrapped deflate to match .NET HTTP decompression behavior.
/// </summary>
public class DeflateContent : CompressedContent
{
    /// <summary>
    /// Creates a deflate-compressed content wrapper.
    /// </summary>
    /// <param name="content">Inner content to compress while serializing.</param>
    /// <param name="compressionLevel">Compression level used by the deflate stream.</param>
    /// <param name="timeout">Optional timeout while copying the inner content stream.</param>
    /// <param name="bufferSize">Optional copy buffer size.</param>
    /// <param name="token">Cancellation token used while reading and copying the inner content.</param>
    public DeflateContent(HttpContent content, CompressionLevel compressionLevel,
        TimeSpan? timeout = null, int? bufferSize = null, CancellationToken token = default)
        : base(content, "deflate", compressionLevel, timeout, bufferSize, token)
    {
    }

    protected override Stream CreateCompressedStream(Stream stream)
    {
        // Yes, ZLibStream over DeflateStream.
        // See this note: https://github.com/dotnet/runtime/blob/7ab969c84ef05ba948c0075392716ce335b47744/src/libraries/System.Net.Http/src/System/Net/Http/SocketsHttpHandler/DecompressionHandler.cs#L231
#if !NET5_0_OR_GREATER
        return new DeflateStream(stream, CompressionLevel, true);
#else
        return new ZLibStream(stream, CompressionLevel, true);
#endif
    }
}
