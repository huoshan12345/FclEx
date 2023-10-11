namespace FclEx.Http;

public abstract class CompressedContent : BufferedContent
{
    public CompressionLevel CompressionLevel { get; }
    public abstract string Encoding { get; }

    protected CompressedContent(HttpContent content, CompressionLevel compressionLevel, TimeSpan? timeout = null, int bufferSize = 262144, CancellationToken token = default)
        : base(content, timeout, bufferSize, token)
    {
        CompressionLevel = compressionLevel;
        Headers.Add(HttpKnownHeaderNames.ContentEncoding, Encoding);
    }

    protected abstract Stream CreateCompressedStream(Stream stream);

    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        await using var contentStream = await Content.ReadAsStreamAsync(Token);
        await using var compressedStream = CreateCompressedStream(stream);
        await contentStream.CopyToAsync(compressedStream, BufferSize, Timeout, Token);
    }

    protected override bool TryComputeLength(out long length)
    {
        length = 0;
        return false;
    }
}