namespace FclEx.Http;

public class GZipContent : CompressedContent
{
    public GZipContent(HttpContent content, CompressionLevel compressionLevel,
        TimeSpan? timeout = null, int bufferSize = 262144, CancellationToken token = default)
        : base(content, "gzip", compressionLevel, timeout, bufferSize, token)
    {
    }

    protected override Stream CreateCompressedStream(Stream stream)
    {
        return new GZipStream(stream, CompressionLevel, true);
    }
}