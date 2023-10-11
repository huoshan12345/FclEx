namespace FclEx.Http;

public class DeflateContent : CompressedContent
{
    public DeflateContent(HttpContent content, CompressionLevel compressionLevel, 
        TimeSpan? timeout = null, int bufferSize = 262144, CancellationToken token = default)
        : base(content, compressionLevel, timeout, bufferSize, token)
    {
    }

    public override string Encoding { get; } = "deflate";

    protected override Stream CreateCompressedStream(Stream stream)
    {
        return new DeflateStream(stream, CompressionLevel, true);
    }
}