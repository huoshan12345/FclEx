namespace FclEx.Http;

public class GZipContent : CompressedContent
{
    public GZipContent(HttpContent content, CompressionLevel compressionLevel,
        TimeSpan? timeout = null, int bufferSize = 262144, CancellationToken token = default)
        : base(content, compressionLevel, timeout, bufferSize, token)
    {
    }

    public override string Encoding { get; } = "gzip";

    protected override Stream CreateCompressedStream(Stream stream)
    {
        return new GZipStream(stream, CompressionLevel, true);
    }
}