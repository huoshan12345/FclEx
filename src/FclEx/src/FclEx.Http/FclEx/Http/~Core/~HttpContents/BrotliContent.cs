namespace FclEx.Http;

public class BrotliContent : CompressedContent
{
    public BrotliContent(HttpContent content, CompressionLevel compressionLevel,
        TimeSpan? timeout = null, int bufferSize = 262144, CancellationToken token = default)
        : base(content, compressionLevel, timeout, bufferSize, token)
    {
    }

    public override string Encoding { get; } = "br";

    protected override Stream CreateCompressedStream(Stream stream)
    {
        return new BrotliStream(stream, CompressionLevel, true);
    }
}