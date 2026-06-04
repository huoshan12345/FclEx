namespace FclEx.Http;

public abstract class CompressedContent : HttpContent
{
    public HttpContent Content { get; }
    public int BufferSize { get; }
    public TimeSpan? Timeout { get; }
    public CancellationToken Token { get; }
    public CompressionLevel CompressionLevel { get; }

    protected CompressedContent(HttpContent content, string encoding, CompressionLevel compressionLevel,
        TimeSpan? timeout = null, int bufferSize = 262144, CancellationToken token = default)
    {
        Content = content;
        Timeout = timeout;
        Token = token;
        BufferSize = bufferSize;
        CompressionLevel = compressionLevel;
        content.Headers.CopyTo(Headers, HttpHeaderNames.ContentLength, HttpHeaderNames.ContentEncoding);
        Headers.Add(HttpHeaderNames.ContentEncoding, encoding);
    }

    protected abstract Stream CreateCompressedStream(Stream stream);

    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        var contentStream = await Content.ReadAsStreamAsync(Token);
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